using HobbyApp.Application.Tasks.Models;

namespace HobbyApp.Application.Tasks;

/// <summary>
/// CRUD operations for the current user's tasks and their checklists.
/// Scoped to the authenticated user; a missing/other-user task is reported as
/// not found (null/false).
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(TaskQuery query, CancellationToken cancellationToken = default);

    Task<TaskDto?> GetTaskAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskDto?> SetCompletedAsync(Guid id, bool isCompleted, CancellationToken cancellationToken = default);

    Task<bool> TrashAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskDto?> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
}
