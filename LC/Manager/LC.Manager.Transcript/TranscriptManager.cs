using LC.Access.Notion;
using LC.Access.Notion.Models;
using LC.Access.Notion.Requests;
using LC.Manager.Transcript.Models;
using LC.Manager.Transcript.Requests;

namespace LC.Manager.Transcript;

public class TranscriptManager : ITranscriptManager
{
    private readonly INotionTranscriptAccessor _accessor;

    public TranscriptManager(INotionTranscriptAccessor accessor) => _accessor = accessor;

    public async Task<IReadOnlyList<TranscriptListItemModel>> GetTranscriptsAsync(
        GetTranscriptsRequest request, CancellationToken cancellationToken = default)
    {
        var results = await _accessor.GetTranscriptsAsync(
            new GetTranscriptsAccessRequest(request.RepName, request.DealStage, request.Reviewed),
            cancellationToken);

        return results.Select(Map).ToList();
    }

    public async Task<TranscriptModel?> GetTranscriptAsync(
        string pageId, CancellationToken cancellationToken = default)
    {
        var result = await _accessor.GetTranscriptAsync(pageId, cancellationToken);
        return result is null ? null : Map(result);
    }

    private static TranscriptListItemModel Map(NotionTranscriptListItemAccessModel m) => new(
        m.PageId, m.RecordingName, m.Company, m.RepName, m.DealStage,
        m.CallType, m.Audience, m.DurationMins, m.CreatedAt, m.Reviewed);

    private static TranscriptModel Map(NotionTranscriptAccessModel m) => new(
        m.PageId, m.RecordingName, m.OpportunityId, m.Company, m.ContactName, m.ContactTitle,
        m.RepName, m.DealStage, m.CallType, m.Outcome, m.Audience, m.RecordingId, m.Speakers,
        m.DeviceSerial, m.DurationMins, m.CreatedAt, m.Reviewed, m.KeyTopics, m.ObjectionsRaised,
        m.CoachNotes, m.CallQuality, m.CoachableMoments, m.SyncedByName, m.TranscriptBody);
}
