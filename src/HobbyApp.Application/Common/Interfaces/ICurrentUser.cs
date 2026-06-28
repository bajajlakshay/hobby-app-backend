namespace HobbyApp.Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user. Implemented by the
/// Web layer over the incoming request's claims.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }
}
