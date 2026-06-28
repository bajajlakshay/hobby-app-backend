using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.Tasks.Models;

public sealed record ChecklistItemDto(string Id, string Text, bool IsCompleted);

public sealed record TaskDto(
    Guid Id,
    string Title,
    IReadOnlyList<ChecklistItemDto> Items,
    int CompletedCount,
    int TotalCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static TaskDto FromEntity(TaskItem task)
    {
        var items = task.Items
            .Select(i => new ChecklistItemDto(i.Id, i.Text, i.IsCompleted))
            .ToList();

        return new TaskDto(
            task.Id,
            task.Title,
            items,
            items.Count(i => i.IsCompleted),
            items.Count,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
