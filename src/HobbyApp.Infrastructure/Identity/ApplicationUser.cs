using Microsoft.AspNetCore.Identity;

namespace HobbyApp.Infrastructure.Identity;

/// <summary>
/// Application user backed by ASP.NET Core Identity, keyed by <see cref="Guid"/>.
/// Add profile fields (DisplayName, etc.) here as the app grows.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>SHA-256 hash (hex) of the current email-verification OTP, if any.</summary>
    public string? EmailOtpHash { get; set; }

    /// <summary>When the current OTP expires. Null when no OTP is outstanding.</summary>
    public DateTimeOffset? EmailOtpExpiresAt { get; set; }

    /// <summary>Failed verification attempts against the current OTP (brute-force guard).</summary>
    public int EmailOtpAttempts { get; set; }
}
