using LC.Host.Api.ApiModels;
using LC.Manager.Transcript;
using LC.Manager.Transcript.Models;
using LC.Manager.Transcript.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LC.Host.Api.Controllers;

[ApiController]
[Route("api/transcripts")]
[Authorize]
public class TranscriptController : ControllerBase
{
    private readonly ITranscriptManager _manager;

    public TranscriptController(ITranscriptManager manager) => _manager = manager;

    [HttpGet]
    public async Task<IActionResult> GetTranscripts([FromQuery] GetTranscriptsApiRequest request, CancellationToken ct)
    {
        var transcripts = await _manager.GetTranscriptsAsync(
            new GetTranscriptsRequest(request.RepName, request.DealStage, request.Reviewed), ct);

        return Ok(transcripts.Select(MapToListItem));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTranscript(string id, CancellationToken ct)
    {
        var transcript = await _manager.GetTranscriptAsync(id, ct);
        return transcript is null ? NotFound() : Ok(MapToDetail(transcript));
    }

    private static TranscriptListItemApiModel MapToListItem(TranscriptListItemModel m) => new(
        m.PageId, m.RecordingName, m.Company, m.RepName, m.DealStage,
        m.CallType, m.Audience, m.DurationMins, m.CreatedAt, m.Reviewed);

    private static TranscriptApiModel MapToDetail(TranscriptModel m) => new(
        m.PageId, m.RecordingName, m.OpportunityId, m.Company, m.ContactName, m.ContactTitle,
        m.RepName, m.DealStage, m.CallType, m.Outcome, m.Audience, m.RecordingId, m.Speakers,
        m.DeviceSerial, m.DurationMins, m.CreatedAt, m.Reviewed, m.KeyTopics, m.ObjectionsRaised,
        m.CoachNotes, m.CallQuality, m.CoachableMoments, m.SyncedByName, m.TranscriptBody);
}
