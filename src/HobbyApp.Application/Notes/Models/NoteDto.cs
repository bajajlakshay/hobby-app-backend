using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.Notes.Models;

/// <summary>Note as returned to clients.</summary>
public sealed record NoteDto(
    Guid Id,
    string Title,
    string Content,
    string PlainText,
    string? Color,
    bool IsPinned,
    bool IsArchived,
    bool IsTrashed,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static NoteDto FromEntity(Note note) => new(
        note.Id,
        note.Title,
        note.Content,
        note.PlainText,
        note.Color,
        note.IsPinned,
        note.IsArchived,
        note.DeletedAt is not null,
        note.CreatedAt,
        note.UpdatedAt);
}
