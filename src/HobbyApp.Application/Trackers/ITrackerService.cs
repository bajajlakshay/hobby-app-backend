using HobbyApp.Application.Trackers.Models;

namespace HobbyApp.Application.Trackers;

/// <summary>
/// Habit/goal tracking for the current user. Scoped to the authenticated user;
/// another user's tracker is reported as not found (null/false).
/// </summary>
public interface ITrackerService
{
    Task<IReadOnlyList<TrackerDto>> GetTrackersAsync(bool includeArchived, CancellationToken cancellationToken = default);

    /// <summary>Creates the tracker with the given id, or updates it if it exists. Null if the id belongs to another user.</summary>
    Task<TrackerDto?> UpsertTrackerAsync(Guid id, SaveTrackerRequest request, CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a tracker and all of its entries.</summary>
    Task<bool> DeleteTrackerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntryDto>> GetEntriesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>Sets the day's value; a value of 0 clears the entry. False if the tracker is not found.</summary>
    Task<bool> SetEntryAsync(Guid trackerId, DateOnly day, int value, CancellationToken cancellationToken = default);

    /// <summary>Applies many entries at once (initial sync). Entries for unknown trackers are skipped; returns the number applied.</summary>
    Task<int> SetEntriesAsync(IReadOnlyList<EntryInput> entries, CancellationToken cancellationToken = default);
}
