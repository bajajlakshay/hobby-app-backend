namespace HobbyApp.Application.Authentication.Models;

/// <summary>
/// Returned to the client on successful authentication. The client stores the
/// access token for API calls and uses the refresh token to obtain a new one.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);
