using System.Security.Claims;
using HobbyApp.Application.Common.Interfaces;

namespace HobbyApp.Api.Services;

/// <summary>
/// Reads the authenticated user's identity from the current request's claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
}
