using HobbyApp.Application.Authentication.Models;
using HobbyApp.Application.Common.Models;

namespace HobbyApp.Application.Authentication;

/// <summary>
/// Contract for authentication operations. Implemented in Infrastructure over
/// ASP.NET Core Identity, keeping the rest of the app free of Identity types.
/// </summary>
public interface IIdentityService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
