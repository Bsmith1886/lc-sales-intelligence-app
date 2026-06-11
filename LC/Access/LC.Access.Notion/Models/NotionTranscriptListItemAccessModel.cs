namespace LC.Access.Notion.Models;

public record NotionTranscriptListItemAccessModel(
    string PageId,
    string RecordingName,
    string? Company,
    string? RepName,
    string? DealStage,
    string? CallType,
    string? Audience,
    double? DurationMins,
    DateTime? CreatedAt,
    bool Reviewed
);
