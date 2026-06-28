using HobbyApp.Application.Notes.Models;

namespace HobbyApp.Application.Notes;

/// <summary>
/// CRUD and organization operations for the current user's notes.
/// All operations are scoped to the authenticated user; a note that does not
/// exist or belongs to another user is reported as not found (null/false).
/// </summary>
public interface INoteService
{
    Task<IReadOnlyList<NoteDto>> GetNotesAsync(NoteQuery query, CancellationToken cancellationToken = default);

    Task<NoteDto?> GetNoteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NoteDto> CreateNoteAsync(CreateNoteRequest request, CancellationToken cancellationToken = default);

    Task<NoteDto?> UpdateNoteAsync(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task<NoteDto?> SetPinnedAsync(Guid id, bool isPinned, CancellationToken cancellationToken = default);

    Task<NoteDto?> SetArchivedAsync(Guid id, bool isArchived, CancellationToken cancellationToken = default);

    /// <summary>Moves a note to trash (soft delete).</summary>
    Task<bool> TrashAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Restores a trashed note back to active.</summary>
    Task<NoteDto?> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Permanently removes a note.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
