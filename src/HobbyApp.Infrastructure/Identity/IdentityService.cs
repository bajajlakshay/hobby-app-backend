using System.Security.Cryptography;
using System.Text;
using HobbyApp.Application.Authentication;
using HobbyApp.Application.Authentication.Models;
using HobbyApp.Application.Common.Interfaces;
using HobbyApp.Application.Common.Models;
using HobbyApp.Infrastructure.Authentication;
using HobbyApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HobbyApp.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    JwtTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IOptions<JwtSettings> jwtOptions) : IIdentityService
{
    private const int OtpLength = 6;
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
    private const int MaxOtpAttempts = 5;
    private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
    /// <summary>How long expired/rotated refresh tokens are kept before purging.</summary>
    private static readonly TimeSpan RefreshTokenRetention = TimeSpan.FromDays(30);

    private readonly JwtSettings _settings = jwtOptions.Value;

    public async Task<Result<bool>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Result<bool>.Failure("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result<bool>.Failure(result.Errors.Select(e => e.Description));
        }

        await GenerateAndSendOtpAsync(user, cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<AuthResponse>> VerifyEmailAsync(
        VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<AuthResponse>.Failure("Invalid or expired code.");
        }

        if (user.EmailConfirmed)
        {
            return Result<AuthResponse>.Failure("Email is already verified. Please sign in.");
        }

        if (user.EmailOtpHash is null || user.EmailOtpExpiresAt is null ||
            user.EmailOtpExpiresAt < DateTimeOffset.UtcNow)
        {
            return Result<AuthResponse>.Failure("Your code has expired. Please request a new one.");
        }

        if (user.EmailOtpAttempts >= MaxOtpAttempts)
        {
            return Result<AuthResponse>.Failure("Too many attempts. Please request a new code.");
        }

        if (!CodeMatches(request.Code, user.EmailOtpHash))
        {
            user.EmailOtpAttempts++;
            await userManager.UpdateAsync(user);
            return Result<AuthResponse>.Failure("Invalid code.");
        }

        // Success: confirm the email and clear the outstanding OTP.
        user.EmailConfirmed = true;
        user.EmailOtpHash = null;
        user.EmailOtpExpiresAt = null;
        user.EmailOtpAttempts = 0;
        await userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(await IssueTokensAsync(user, cancellationToken));
    }

    public async Task<Result<bool>> ResendOtpAsync(
        ResendOtpRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<bool>.Failure("No account found for this email.");
        }

        if (user.EmailConfirmed)
        {
            return Result<bool>.Failure("Email is already verified. Please sign in.");
        }

        // Cooldown between sends, so the endpoint can't be used to flood an inbox.
        if (user.EmailOtpExpiresAt is { } expiresAt)
        {
            var lastSentAt = expiresAt - OtpLifetime;
            if (DateTimeOffset.UtcNow - lastSentAt < OtpResendCooldown)
            {
                return Result<bool>.Failure(
                    "A code was just sent. Please wait a minute before requesting another.");
            }
        }

        await GenerateAndSendOtpAsync(user, cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<LoginResult>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<LoginResult>.Failure("Invalid email or password.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result<LoginResult>.Failure(
                "Too many failed attempts. Please try again in a few minutes.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            // Counts toward the lockout threshold configured in DI.
            await userManager.AccessFailedAsync(user);
            return Result<LoginResult>.Failure("Invalid email or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
        {
            // Credentials are valid but the account isn't verified yet.
            return Result<LoginResult>.Success(new LoginResult(null, RequiresEmailVerification: true));
        }

        var tokens = await IssueTokensAsync(user, cancellationToken);
        return Result<LoginResult>.Success(new LoginResult(tokens, RequiresEmailVerification: false));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var stored = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (stored is null)
        {
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        if (!stored.IsActive)
        {
            if (stored.RevokedAt is not null)
            {
                // Reuse of a rotated token: either the token was stolen or a
                // client retried after a lost response. Revoke every active
                // token for the user so a possible thief is cut off too.
                var now = DateTimeOffset.UtcNow;
                await context.RefreshTokens
                    .Where(rt => rt.UserId == stored.UserId && rt.RevokedAt == null && rt.ExpiresAt > now)
                    .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, now), cancellationToken);
            }
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return Result<AuthResponse>.Failure("User no longer exists.");
        }

        // Opportunistic cleanup: the audit trail doesn't need tokens this stale.
        var purgeBefore = DateTimeOffset.UtcNow - RefreshTokenRetention;
        await context.RefreshTokens
            .Where(rt => rt.UserId == stored.UserId && rt.ExpiresAt < purgeBefore)
            .ExecuteDeleteAsync(cancellationToken);

        // Rotate: revoke the presented token and issue a fresh pair atomically.
        stored.RevokedAt = DateTimeOffset.UtcNow;
        return Result<AuthResponse>.Success(await IssueTokensAsync(user, cancellationToken));
    }

    public async Task<Result<bool>> RevokeRefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return Result<bool>.Failure("Invalid refresh token.");
        }

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private async Task GenerateAndSendOtpAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var code = GenerateOtp();
        user.EmailOtpHash = HashOtp(code);
        user.EmailOtpExpiresAt = DateTimeOffset.UtcNow.Add(OtpLifetime);
        user.EmailOtpAttempts = 0;
        await userManager.UpdateAsync(user);

        await emailSender.SendAsync(
            user.Email!,
            "Your HobbyApp verification code",
            BuildOtpEmail(code),
            cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = tokenGenerator.GenerateAccessToken(user);

        var refreshToken = new RefreshToken
        {
            Token = tokenGenerator.GenerateRefreshToken(),
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken.Token, expiresAt);
    }

    private static string GenerateOtp()
    {
        // Cryptographically strong 6-digit code (000000-999999).
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString($"D{OtpLength}");
    }

    private static string HashOtp(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    private static bool CodeMatches(string code, string expectedHash)
    {
        var actual = Encoding.UTF8.GetBytes(HashOtp(code));
        var expected = Encoding.UTF8.GetBytes(expectedHash);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static string BuildOtpEmail(string code) =>
        $"""
        <div style="font-family:sans-serif;max-width:480px;margin:auto">
          <h2>Verify your email</h2>
          <p>Use this code to finish setting up your HobbyApp account:</p>
          <p style="font-size:32px;font-weight:bold;letter-spacing:6px">{code}</p>
          <p>This code expires in 10 minutes. If you didn't request it, you can ignore this email.</p>
        </div>
        """;
}
