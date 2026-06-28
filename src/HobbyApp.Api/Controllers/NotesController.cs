using HobbyApp.Application.Notes;
using HobbyApp.Application.Notes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HobbyApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notes")]
public class NotesController(INoteService notes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NoteDto>>> List(
        [FromQuery] NoteView view = NoteView.Active,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await notes.GetNotesAsync(new NoteQuery(view, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var note = await notes.GetNoteAsync(id, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create(
        CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var note = await notes.CreateNoteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = note.Id }, note);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NoteDto>> Update(
        Guid id, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var note = await notes.UpdateNoteAsync(id, request, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPut("{id:guid}/pinned")]
    public async Task<ActionResult<NoteDto>> SetPinned(
        Guid id, [FromBody] bool isPinned, CancellationToken cancellationToken)
    {
        var note = await notes.SetPinnedAsync(id, isPinned, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPut("{id:guid}/archived")]
    public async Task<ActionResult<NoteDto>> SetArchived(
        Guid id, [FromBody] bool isArchived, CancellationToken cancellationToken)
    {
        var note = await notes.SetArchivedAsync(id, isArchived, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>Moves a note to trash (soft delete).</summary>
    [HttpPost("{id:guid}/trash")]
    public async Task<IActionResult> Trash(Guid id, CancellationToken cancellationToken)
    {
        var trashed = await notes.TrashAsync(id, cancellationToken);
        return trashed ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<NoteDto>> Restore(Guid id, CancellationToken cancellationToken)
    {
        var note = await notes.RestoreAsync(id, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>Permanently deletes a note.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await notes.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
