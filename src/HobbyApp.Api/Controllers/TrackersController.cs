using HobbyApp.Application.Trackers;
using HobbyApp.Application.Trackers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HobbyApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/trackers")]
public class TrackersController(ITrackerService trackers) : ControllerBase
{
    /// <summary>Largest date range a single entries query may cover.</summary>
    private const int MaxRangeDays = 800;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrackerDto>>> List(
        [FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        return Ok(await trackers.GetTrackersAsync(includeArchived, cancellationToken));
    }

    /// <summary>Create or update a tracker. The id is chosen by the client.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TrackerDto>> Upsert(
        Guid id, SaveTrackerRequest request, CancellationToken cancellationToken)
    {
        var tracker = await trackers.UpsertTrackerAsync(id, request, cancellationToken);
        return tracker is null ? NotFound() : Ok(tracker);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await trackers.DeleteTrackerAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("entries")]
    public async Task<ActionResult<IReadOnlyList<EntryDto>>> Entries(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        if (to < from || to.DayNumber - from.DayNumber > MaxRangeDays)
        {
            return BadRequest($"'to' must be on/after 'from' and within {MaxRangeDays} days.");
        }

        return Ok(await trackers.GetEntriesAsync(from, to, cancellationToken));
    }

    /// <summary>Log a value for one day. A value of 0 clears the day.</summary>
    [HttpPut("{id:guid}/entries/{day}")]
    public async Task<IActionResult> SetEntry(
        Guid id, DateOnly day, SetEntryRequest request, CancellationToken cancellationToken)
    {
        var ok = await trackers.SetEntryAsync(id, day, request.Value, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Bulk upsert, used to push locally logged history in one call.</summary>
    [HttpPost("entries/batch")]
    public async Task<ActionResult<int>> SetEntries(
        [FromBody] List<EntryInput> entries, CancellationToken cancellationToken)
    {
        if (entries.Count > 5000)
        {
            return BadRequest("At most 5000 entries per batch.");
        }

        return Ok(await trackers.SetEntriesAsync(entries, cancellationToken));
    }
}
