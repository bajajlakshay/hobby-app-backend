using HobbyApp.Application.Common.Interfaces;
using HobbyApp.Application.Trackers.Models;
using HobbyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Application.Trackers;

internal sealed class TrackerService(IApplicationDbContext context, ICurrentUser currentUser) : ITrackerService
{
    private Guid UserId =>
        currentUser.UserId ?? throw new UnauthorizedAccessException("No authenticated user.");

    public async Task<IReadOnlyList<TrackerDto>> GetTrackersAsync(
        bool includeArchived, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var trackers = context.Trackers.AsNoTracking().Where(t => t.UserId == userId);
        if (!includeArchived)
        {
            trackers = trackers.Where(t => !t.IsArchived);
        }

        var results = await trackers.OrderBy(t => t.CreatedAt).ToListAsync(cancellationToken);
        return results.Select(TrackerDto.FromEntity).ToList();
    }

    public async Task<TrackerDto?> UpsertTrackerAsync(
        Guid id, SaveTrackerRequest request, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var tracker = await context.Trackers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tracker is null)
        {
            tracker = new Tracker { Id = id, UserId = userId, CreatedAt = DateTimeOffset.UtcNow };
            context.Trackers.Add(tracker);
        }
        else if (tracker.UserId != userId)
        {
            return null;
        }
        else
        {
            tracker.UpdatedAt = DateTimeOffset.UtcNow;
        }

        tracker.Name = request.Name.Trim();
        tracker.Kind = request.Kind;
        tracker.Target = request.Kind == TrackerKind.Check ? 1 : request.Target;
        tracker.Unit = request.Unit?.Trim() ?? string.Empty;
        tracker.DaysPerWeek = request.DaysPerWeek;
        tracker.Color = request.Color;
        tracker.IsArchived = request.IsArchived;

        await context.SaveChangesAsync(cancellationToken);
        return TrackerDto.FromEntity(tracker);
    }

    public async Task<bool> DeleteTrackerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var tracker = await context.Trackers
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
        if (tracker is null)
        {
            return false;
        }

        context.Trackers.Remove(tracker); // entries cascade
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<EntryDto>> GetEntriesAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var entries = await context.TrackerEntries.AsNoTracking()
            .Where(e => e.UserId == userId && e.Day >= from && e.Day <= to)
            .OrderBy(e => e.Day)
            .ToListAsync(cancellationToken);
        return entries.Select(EntryDto.FromEntity).ToList();
    }

    public async Task<bool> SetEntryAsync(
        Guid trackerId, DateOnly day, int value, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var owns = await context.Trackers.AnyAsync(t => t.Id == trackerId && t.UserId == userId, cancellationToken);
        if (!owns)
        {
            return false;
        }

        await ApplyAsync(userId, trackerId, day, value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> SetEntriesAsync(
        IReadOnlyList<EntryInput> entries, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        var ownedIds = await context.Trackers
            .Where(t => t.UserId == userId)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        var owned = ownedIds.ToHashSet();

        var applied = 0;
        // Last write wins if the batch repeats a (tracker, day) pair.
        foreach (var e in entries.Where(e => owned.Contains(e.TrackerId)))
        {
            await ApplyAsync(userId, e.TrackerId, e.Day, e.Value, cancellationToken);
            applied++;
        }

        await context.SaveChangesAsync(cancellationToken);
        return applied;
    }

    private async Task ApplyAsync(Guid userId, Guid trackerId, DateOnly day, int value, CancellationToken ct)
    {
        // Check the change tracker first so entries added earlier in this unit of work are seen.
        var entry = context.TrackerEntries.Local.FirstOrDefault(e => e.TrackerId == trackerId && e.Day == day)
            ?? await context.TrackerEntries.FirstOrDefaultAsync(e => e.TrackerId == trackerId && e.Day == day, ct);

        if (value <= 0)
        {
            if (entry is not null)
            {
                context.TrackerEntries.Remove(entry);
            }
            return;
        }

        if (entry is null)
        {
            context.TrackerEntries.Add(new TrackerEntry
            {
                TrackerId = trackerId,
                UserId = userId,
                Day = day,
                Value = value,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            entry.Value = value;
            entry.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
