using LC.Manager.Transcript.Models;
using LC.Manager.Transcript.Requests;

namespace LC.Manager.Transcript;

public interface ITranscriptManager
{
    Task<IReadOnlyList<TranscriptListItemModel>> GetTranscriptsAsync(
        GetTranscriptsRequest request, CancellationToken cancellationToken = default);

    Task<TranscriptModel?> GetTranscriptAsync(
        string pageId, CancellationToken cancellationToken = default);
}
