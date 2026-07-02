using HobbyApp.Application.Authentication.Models;
using HobbyApp.Application.Common.Models;

namespace HobbyApp.Application.Authentication;

/// <summary>
/// Contract for authentication operations. Implemented in Infrastructure over
/// ASP.NET Core Identity, keeping the rest of the app free of Identity types.
/// </summary>
public interface IIdentityService
{
    /// <summary>Creates the account (unverified) and emails a one-time verification code.</summary>
    Task<Result<bool>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Verifies the emailed OTP; on success confirms the email and issues tokens.</summary>
    Task<Result<AuthResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>Regenerates and re-sends a verification code for an unverified account.</summary>
    Task<Result<bool>> ResendOtpAsync(ResendOtpRequest request, CancellationToken cancellationToken = default);

    Task<Result<LoginResult>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
