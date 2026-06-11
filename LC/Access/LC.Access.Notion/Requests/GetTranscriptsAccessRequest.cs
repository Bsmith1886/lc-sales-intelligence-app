namespace LC.Access.Notion.Requests;

public record GetTranscriptsAccessRequest(
    string? RepName = null,
    string? DealStage = null,
    bool? Reviewed = null
);
