namespace LC.Host.Api.ApiModels;

public record TranscriptListItemApiModel(
    string Id,
    string Name,
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
