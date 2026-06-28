using Microsoft.AspNetCore.Identity;

namespace HobbyApp.Infrastructure.Identity;

/// <summary>
/// Application user backed by ASP.NET Core Identity, keyed by <see cref="Guid"/>.
/// Add profile fields (DisplayName, etc.) here as the app grows.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
