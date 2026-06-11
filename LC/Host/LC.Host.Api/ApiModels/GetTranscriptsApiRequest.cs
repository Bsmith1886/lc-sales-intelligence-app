namespace LC.Host.Api.ApiModels;

public record GetTranscriptsApiRequest
{
    public string? RepName { get; init; }
    public string? DealStage { get; init; }
    public bool? Reviewed { get; init; }
}
