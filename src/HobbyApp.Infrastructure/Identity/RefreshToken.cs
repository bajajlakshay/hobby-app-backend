using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyApp.Infrastructure.Identity;

/// <summary>
/// A server-side refresh token. One row per issued token; rotated on use and
/// marked revoked rather than deleted to preserve an audit trail.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    [NotMapped]
    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
}
