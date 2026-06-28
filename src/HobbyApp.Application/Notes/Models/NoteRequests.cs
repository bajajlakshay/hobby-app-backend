using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Application.Notes.Models;

public sealed record CreateNoteRequest(
    [MaxLength(512)] string? Title,
    string? Content,
    string? PlainText,
    [MaxLength(32)] string? Color);

public sealed record UpdateNoteRequest(
    [MaxLength(512)] string? Title,
    string? Content,
    string? PlainText,
    [MaxLength(32)] string? Color);

/// <summary>Which slice of the user's notes to return.</summary>
public enum NoteView
{
    Active,
    Archived,
    Trash,
}

public sealed record NoteQuery(NoteView View = NoteView.Active, string? Search = null);
