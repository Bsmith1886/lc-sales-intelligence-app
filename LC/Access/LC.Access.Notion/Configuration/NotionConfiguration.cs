namespace LC.Access.Notion.Configuration;

public class NotionConfiguration
{
    public string ApiToken { get; set; } = string.Empty;
    public string TranscriptsDatabaseId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.notion.com/v1";
}
