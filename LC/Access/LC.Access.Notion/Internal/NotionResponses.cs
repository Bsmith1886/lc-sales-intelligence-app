using System.Text.Json;
using System.Text.Json.Serialization;

namespace LC.Access.Notion.Internal;

internal class NotionQueryResponse
{
    [JsonPropertyName("results")]
    public List<NotionPageResponse> Results { get; set; } = [];

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }
}

internal class NotionPageResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created_time")]
    public DateTime CreatedTime { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}

internal class NotionBlocksResponse
{
    [JsonPropertyName("results")]
    public List<NotionBlockResponse> Results { get; set; } = [];
}

internal class NotionBlockResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("paragraph")]
    public NotionRichTextContent? Paragraph { get; set; }

    [JsonPropertyName("heading_1")]
    public NotionRichTextContent? Heading1 { get; set; }

    [JsonPropertyName("heading_2")]
    public NotionRichTextContent? Heading2 { get; set; }

    [JsonPropertyName("heading_3")]
    public NotionRichTextContent? Heading3 { get; set; }
}

internal class NotionRichTextContent
{
    [JsonPropertyName("rich_text")]
    public List<NotionRichTextItem> RichText { get; set; } = [];
}

internal class NotionRichTextItem
{
    [JsonPropertyName("plain_text")]
    public string PlainText { get; set; } = string.Empty;
}
