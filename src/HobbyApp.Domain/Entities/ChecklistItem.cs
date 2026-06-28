namespace HobbyApp.Domain.Entities;

/// <summary>
/// A single checklist entry within a <see cref="TaskItem"/>. Stored as part of
/// the task's JSON column; order is the position in the list.
/// </summary>
public class ChecklistItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}
