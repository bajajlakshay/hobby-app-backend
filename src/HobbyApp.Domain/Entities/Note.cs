using HobbyApp.Domain.Common;

namespace HobbyApp.Domain.Entities;

/// <summary>
/// A note owned by a user. A note is a block-based document: <see cref="Content"/>
/// holds an ordered list of typed-text and handwriting blocks serialized as JSON,
/// opaque to the backend. <see cref="PlainText"/> is a flattened copy of the typed
/// text used for search.
/// </summary>
public class Note : BaseAuditableEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>JSON array of content blocks (defined by the client).</summary>
    public string Content { get; set; } = "[]";

    /// <summary>Flattened typed text, maintained by the client for search.</summary>
    public string PlainText { get; set; } = string.Empty;

    /// <summary>Optional background color (palette key or hex), null for default.</summary>
    public string? Color { get; set; }

    public bool IsPinned { get; set; }

    public bool IsArchived { get; set; }

    /// <summary>Set when the note is moved to trash (soft delete); null otherwise.</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
