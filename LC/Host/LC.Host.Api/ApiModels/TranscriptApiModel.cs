namespace LC.Host.Api.ApiModels;

public record TranscriptApiModel(
    string Id,
    string Name,
    string? OpportunityId,
    string? Company,
    string? ContactName,
    string? ContactTitle,
    string? RepName,
    string? DealStage,
    string? DealType,
    string? Outcome,
    string? Audience,
    string? RecordingId,
    string? Speakers,
    string? DeviceSerial,
    double? Duration,
    DateTime? CreatedAt,
    bool Reviewed,
    string[] KeyTopics,
    string? ObjectionsRaised,
    string? CoachNotes,
    string? CallQuality,
    bool CoachableMoments,
    string? SyncedByName,
    string TranscriptText
);
