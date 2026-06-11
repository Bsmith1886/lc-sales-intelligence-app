namespace LC.Access.Notion.Requests;

public record UpdateTranscriptAccessRequest(
    string? OpportunityId = null,
    string? Company = null,
    string? ContactName = null,
    string? ContactTitle = null,
    string? RepName = null,
    string? DealStage = null,
    string? Outcome = null,
    string? CoachNotes = null,
    string? CallQuality = null,
    bool? CoachableMoments = null,
    bool? Reviewed = null
);
