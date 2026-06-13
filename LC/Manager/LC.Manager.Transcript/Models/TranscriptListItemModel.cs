namespace LC.Manager.Transcript.Models;

public record TranscriptListItemModel(
    string PageId,
    string RecordingName,
    string? Company,
    string? RepName,
    string? DealStage,
    string? CallType,
    string? Audience,
    double? DurationMins,
    DateTime? CreatedAt,
    bool Reviewed,
    string? CallQuality,
    bool CoachableMoments
);
