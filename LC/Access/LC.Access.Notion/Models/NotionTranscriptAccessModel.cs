namespace LC.Access.Notion.Models;

public record NotionTranscriptAccessModel(
    string PageId,
    string RecordingName,
    string? OpportunityId,
    string? Company,
    string? ContactName,
    string? ContactTitle,
    string? RepName,
    string? DealStage,
    string? CallType,
    string? Outcome,
    string? Audience,
    string? RecordingId,
    string? Speakers,
    string? DeviceSerial,
    double? DurationMins,
    DateTime? CreatedAt,
    bool Reviewed,
    string[] KeyTopics,
    string? ObjectionsRaised,
    string? CoachNotes,
    string? CallQuality,
    bool CoachableMoments,
    string? SyncedByName,
    string TranscriptBody
);
