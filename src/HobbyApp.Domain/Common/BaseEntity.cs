namespace HobbyApp.Domain.Common;

/// <summary>
/// Base type for all domain entities. Identity is defined by <see cref="Id"/>.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
