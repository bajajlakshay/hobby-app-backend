namespace HobbyApp.Application.Authentication.Models;

/// <summary>
/// Returned to the client on successful authentication. The client stores the
/// access token for API calls and uses the refresh token to obtain a new one.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);

/// <summary>
/// Outcome of a login attempt. When <see cref="RequiresEmailVerification"/> is true,
/// <see cref="Tokens"/> is null and the client must complete OTP verification first.
/// </summary>
public sealed record LoginResult(
    AuthResponse? Tokens,
    bool RequiresEmailVerification);
