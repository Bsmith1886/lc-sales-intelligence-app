using LC.Access.Notion.Models;
using LC.Access.Notion.Requests;

namespace LC.Access.Notion;

public interface INotionTranscriptAccessor
{
    Task<IReadOnlyList<NotionTranscriptListItemAccessModel>> GetTranscriptsAsync(
        GetTranscriptsAccessRequest request, CancellationToken cancellationToken = default);

    Task<NotionTranscriptAccessModel?> GetTranscriptAsync(
        string pageId, CancellationToken cancellationToken = default);

    Task UpdateTranscriptAsync(
        string pageId, UpdateTranscriptAccessRequest request, CancellationToken cancellationToken = default);
}
