using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Application.Notes.Models;

// Content is capped at 1 MB (a heavy drawing note stays well under this) and
// PlainText at 20k chars so a client can't grow the database without bound.
public sealed record CreateNoteRequest(
    [MaxLength(512)] string? Title,
    [MaxLength(1_000_000)] string? Content,
    [MaxLength(20_000)] string? PlainText,
    [MaxLength(32)] string? Color);

public sealed record UpdateNoteRequest(
    [MaxLength(512)] string? Title,
    [MaxLength(1_000_000)] string? Content,
    [MaxLength(20_000)] string? PlainText,
    [MaxLength(32)] string? Color);

/// <summary>Which slice of the user's notes to return.</summary>
public enum NoteView
{
    Active,
    Archived,
    Trash,
}

public sealed record NoteQuery(NoteView View = NoteView.Active, string? Search = null);
