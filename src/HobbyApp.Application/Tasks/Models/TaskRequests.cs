using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Application.Tasks.Models;

public sealed record ChecklistItemInput(
    string? Id,
    [MaxLength(1000)] string Text,
    bool IsCompleted);

public sealed record CreateTaskRequest(
    [MaxLength(512)] string? Title,
    IReadOnlyList<ChecklistItemInput>? Items,
    DateTimeOffset? ReminderAt,
    bool? IsCompleted);

public sealed record UpdateTaskRequest(
    [MaxLength(512)] string? Title,
    IReadOnlyList<ChecklistItemInput>? Items,
    DateTimeOffset? ReminderAt,
    bool? IsCompleted);

public enum TaskView
{
    Active,
    Completed,
    Trash,
}

public sealed record TaskQuery(TaskView View = TaskView.Active, string? Search = null);
