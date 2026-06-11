namespace LC.Manager.Transcript.Requests;

public record GetTranscriptsRequest(
    string? RepName = null,
    string? DealStage = null,
    bool? Reviewed = null
);
