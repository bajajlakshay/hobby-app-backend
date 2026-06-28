using HobbyApp.Domain.Common;

namespace HobbyApp.Domain.Entities;

/// <summary>
/// A user's task with a checklist of sub-items tracking what is done and what
/// is left. Named TaskItem to avoid clashing with System.Threading.Tasks.Task.
/// </summary>
public class TaskItem : BaseAuditableEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<ChecklistItem> Items { get; set; } = [];
}
