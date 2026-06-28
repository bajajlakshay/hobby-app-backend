using HobbyApp.Application.Common.Interfaces;
using HobbyApp.Application.Notes.Models;
using HobbyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Application.Notes;

internal sealed class NoteService(IApplicationDbContext context, ICurrentUser currentUser) : INoteService
{
    private Guid UserId =>
        currentUser.UserId ?? throw new UnauthorizedAccessException("No authenticated user.");

    public async Task<IReadOnlyList<NoteDto>> GetNotesAsync(
        NoteQuery query, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var notes = context.Notes.AsNoTracking().Where(n => n.UserId == userId);

        notes = query.View switch
        {
            NoteView.Active => notes.Where(n => n.DeletedAt == null && !n.IsArchived),
            NoteView.Archived => notes.Where(n => n.DeletedAt == null && n.IsArchived),
            NoteView.Trash => notes.Where(n => n.DeletedAt != null),
            _ => notes,
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            notes = notes.Where(n =>
                n.Title.ToLower().Contains(term) || n.PlainText.ToLower().Contains(term));
        }

        var results = await notes
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt ?? n.CreatedAt)
            .Select(n => NoteDto.FromEntity(n))
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<NoteDto?> GetNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        return note is null ? null : NoteDto.FromEntity(note);
    }

    public async Task<NoteDto> CreateNoteAsync(
        CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = new Note
        {
            UserId = UserId,
            Title = request.Title?.Trim() ?? string.Empty,
            Content = string.IsNullOrWhiteSpace(request.Content) ? "[]" : request.Content,
            PlainText = request.PlainText ?? string.Empty,
            Color = request.Color,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync(cancellationToken);
        return NoteDto.FromEntity(note);
    }

    public async Task<NoteDto?> UpdateNoteAsync(
        Guid id, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        if (note is null)
        {
            return null;
        }

        note.Title = request.Title?.Trim() ?? string.Empty;
        note.Content = string.IsNullOrWhiteSpace(request.Content) ? "[]" : request.Content;
        note.PlainText = request.PlainText ?? string.Empty;
        note.Color = request.Color;
        return await TouchAndSaveAsync(note, cancellationToken);
    }

    public async Task<NoteDto?> SetPinnedAsync(
        Guid id, bool isPinned, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        if (note is null)
        {
            return null;
        }

        note.IsPinned = isPinned;
        return await TouchAndSaveAsync(note, cancellationToken);
    }

    public async Task<NoteDto?> SetArchivedAsync(
        Guid id, bool isArchived, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        if (note is null)
        {
            return null;
        }

        note.IsArchived = isArchived;
        if (isArchived)
        {
            note.IsPinned = false;
        }
        return await TouchAndSaveAsync(note, cancellationToken);
    }

    public async Task<bool> TrashAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        if (note is null)
        {
            return false;
        }

        note.DeletedAt = DateTimeOffset.UtcNow;
        note.IsPinned = false;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<NoteDto?> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        if (note is null)
        {
            return null;
        }

        note.DeletedAt = null;
        note.IsArchived = false;
        return await TouchAndSaveAsync(note, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await FindAsync(id, cancellationToken);
        if (note is null)
        {
            return false;
        }

        context.Notes.Remove(note);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<Note?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = UserId;
        return context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);
    }

    private async Task<NoteDto> TouchAndSaveAsync(Note note, CancellationToken cancellationToken)
    {
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return NoteDto.FromEntity(note);
    }
}
