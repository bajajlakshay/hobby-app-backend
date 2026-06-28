namespace HobbyApp.Domain.Common;

/// <summary>
/// Base type for entities that track creation and modification timestamps.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
