using System.ComponentModel.DataAnnotations;
using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.Trackers.Models;

public sealed record SaveTrackerRequest(
    [Required, MaxLength(128)] string Name,
    TrackerKind Kind,
    [Range(1, 100000)] int Target,
    [MaxLength(32)] string? Unit,
    [Range(1, 7)] int DaysPerWeek,
    long Color,
    bool IsArchived);

public sealed record SetEntryRequest([Range(0, 1000000)] int Value);

public sealed record EntryInput(Guid TrackerId, DateOnly Day, [Range(0, 1000000)] int Value);

public sealed record TrackerDto(
    Guid Id,
    string Name,
    TrackerKind Kind,
    int Target,
    string Unit,
    int DaysPerWeek,
    long Color,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static TrackerDto FromEntity(Tracker t) => new(
        t.Id, t.Name, t.Kind, t.Target, t.Unit, t.DaysPerWeek, t.Color,
        t.IsArchived, t.CreatedAt, t.UpdatedAt);
}

public sealed record EntryDto(Guid TrackerId, DateOnly Day, int Value)
{
    public static EntryDto FromEntity(TrackerEntry e) => new(e.TrackerId, e.Day, e.Value);
}
