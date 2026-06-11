namespace LC.Host.Api.ApiModels;

public record TranscriptListItemApiModel(
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
