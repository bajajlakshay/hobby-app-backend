namespace HobbyApp.Domain.Entities;

/// <summary>
/// The value logged for a tracker on one calendar day. Identity is
/// (<see cref="TrackerId"/>, <see cref="Day"/>), so logging is an upsert.
/// </summary>
public class TrackerEntry
{
    public Guid TrackerId { get; set; }

    public Tracker? Tracker { get; set; }

    /// <summary>Denormalised owner, so entry queries need no join to filter by user.</summary>
    public Guid UserId { get; set; }

    public DateOnly Day { get; set; }

    public int Value { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
