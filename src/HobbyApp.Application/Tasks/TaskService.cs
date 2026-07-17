using HobbyApp.Application.Common.Interfaces;
using HobbyApp.Application.Tasks.Models;
using HobbyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Application.Tasks;

internal sealed class TaskService(IApplicationDbContext context, ICurrentUser currentUser) : ITaskService
{
    private Guid UserId =>
        currentUser.UserId ?? throw new UnauthorizedAccessException("No authenticated user.");

    public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(
        string? search = null, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var tasks = context.Tasks.AsNoTracking().Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            tasks = tasks.Where(t => t.Title.ToLower().Contains(term));
        }

        var results = await tasks
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .ToListAsync(cancellationToken);

        return results.Select(TaskDto.FromEntity).ToList();
    }

    public async Task<TaskDto?> GetTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await FindAsync(id, cancellationToken);
        return task is null ? null : TaskDto.FromEntity(task);
    }

    public async Task<TaskDto> CreateTaskAsync(
        CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            UserId = UserId,
            Title = request.Title?.Trim() ?? string.Empty,
            Items = MapItems(request.Items),
            CreatedAt = DateTimeOffset.UtcNow,
            ReminderAt = request.ReminderAt,
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        return TaskDto.FromEntity(task);
    }

    public async Task<TaskDto?> UpdateTaskAsync(
        Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await FindAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Title = request.Title?.Trim() ?? string.Empty;
        task.Items = MapItems(request.Items);
        task.UpdatedAt = DateTimeOffset.UtcNow;
        task.ReminderAt = request.ReminderAt;
        await context.SaveChangesAsync(cancellationToken);
        return TaskDto.FromEntity(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await FindAsync(id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<TaskItem?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = UserId;
        return context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
    }

    private static List<ChecklistItem> MapItems(IReadOnlyList<ChecklistItemInput>? items) =>
        items?
            .Where(i => !string.IsNullOrWhiteSpace(i.Text))
            .Select(i => new ChecklistItem
            {
                Id = string.IsNullOrWhiteSpace(i.Id) ? Guid.NewGuid().ToString() : i.Id,
                Text = i.Text.Trim(),
                IsCompleted = i.IsCompleted,
            })
            .ToList() ?? [];
}
