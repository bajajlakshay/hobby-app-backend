using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Application.Tasks.Models;

public sealed record ChecklistItemInput(
    string? Id,
    [MaxLength(1000)] string Text,
    bool IsCompleted);

public sealed record CreateTaskRequest(
    [MaxLength(512)] string? Title,
    IReadOnlyList<ChecklistItemInput>? Items,
    DateTimeOffset? ReminderAt);

public sealed record UpdateTaskRequest(
    [MaxLength(512)] string? Title,
    IReadOnlyList<ChecklistItemInput>? Items,
    DateTimeOffset? ReminderAt);
