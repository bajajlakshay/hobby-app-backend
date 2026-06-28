using HobbyApp.Application.Authentication;
using HobbyApp.Application.Authentication.Models;
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
    IOptions<JwtSettings> jwtOptions) : IIdentityService
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Result<AuthResponse>.Failure("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description));
        }

        return Result<AuthResponse>.Success(await IssueTokensAsync(user, cancellationToken));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        return Result<AuthResponse>.Success(await IssueTokensAsync(user, cancellationToken));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var stored = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return Result<AuthResponse>.Failure("User no longer exists.");
        }

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
}
