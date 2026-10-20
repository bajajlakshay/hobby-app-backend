using HobbyApp.Domain.Common;

namespace HobbyApp.Domain.Entities;

/// <summary>
/// Something the user wants to make progress on (a habit or a daily count).
/// The id is supplied by the client so offline-created trackers sync idempotently.
/// </summary>
public class Tracker : BaseAuditableEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public TrackerKind Kind { get; set; }

    /// <summary>Value that makes a day "done". Always 1 for <see cref="TrackerKind.Check"/>.</summary>
    public int Target { get; set; } = 1;

    public string Unit { get; set; } = string.Empty;

    /// <summary>How many days per week meet the goal (1-7).</summary>
    public int DaysPerWeek { get; set; } = 7;

    /// <summary>ARGB colour value chosen by the client.</summary>
    public long Color { get; set; }

    public bool IsArchived { get; set; }

    public List<TrackerEntry> Entries { get; set; } = [];
}

public enum TrackerKind
{
    Check,
    Count,
}
