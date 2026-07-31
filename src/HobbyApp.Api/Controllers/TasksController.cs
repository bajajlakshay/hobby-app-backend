using HobbyApp.Application.Tasks;
using HobbyApp.Application.Tasks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HobbyApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController(ITaskService tasks) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List(
        [FromQuery] TaskView view = TaskView.Active,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await tasks.GetTasksAsync(new TaskQuery(view, search), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var task = await tasks.GetTaskAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(
        CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await tasks.CreateTaskAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(
        Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await tasks.UpdateTaskAsync(id, request, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPut("{id:guid}/completed")]
    public async Task<ActionResult<TaskDto>> SetCompleted(
        Guid id, [FromBody] bool isCompleted, CancellationToken cancellationToken)
    {
        var task = await tasks.SetCompletedAsync(id, isCompleted, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost("{id:guid}/trash")]
    public async Task<IActionResult> Trash(Guid id, CancellationToken cancellationToken)
    {
        var trashed = await tasks.TrashAsync(id, cancellationToken);
        return trashed ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<TaskDto>> Restore(Guid id, CancellationToken cancellationToken)
    {
        var task = await tasks.RestoreAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await tasks.DeleteTaskAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
