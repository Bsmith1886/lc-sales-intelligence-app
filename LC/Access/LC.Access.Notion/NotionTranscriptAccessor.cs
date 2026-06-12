using System.Net.Http.Json;
using System.Text.Json;
using LC.Access.Notion.Configuration;
using LC.Access.Notion.Internal;
using LC.Access.Notion.Models;
using LC.Access.Notion.Requests;
using Microsoft.Extensions.Options;

namespace LC.Access.Notion;

public class NotionTranscriptAccessor : INotionTranscriptAccessor
{
    private readonly HttpClient _httpClient;
    private readonly string _databaseId;

    public NotionTranscriptAccessor(IHttpClientFactory httpClientFactory, IOptions<NotionConfiguration> config)
    {
        _httpClient = httpClientFactory.CreateClient("Notion");
        _databaseId = config.Value.TranscriptsDatabaseId.Trim();
    }

    public async Task<IReadOnlyList<NotionTranscriptListItemAccessModel>> GetTranscriptsAsync(
        GetTranscriptsAccessRequest request, CancellationToken cancellationToken = default)
    {
        var filters = BuildFilters(request);
        var body = filters.Count > 0
            ? (object)new { filter = new { and = filters }, sorts = DefaultSorts(), page_size = 100 }
            : new { sorts = DefaultSorts(), page_size = 100 };

        var response = await _httpClient.PostAsJsonAsync($"databases/{_databaseId}/query", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NotionQueryResponse>(cancellationToken: cancellationToken);
        return result?.Results.Select(MapToListItem).ToList() ?? [];
    }

    public async Task<NotionTranscriptAccessModel?> GetTranscriptAsync(
        string pageId, CancellationToken cancellationToken = default)
    {
        var pageTask = _httpClient.GetFromJsonAsync<NotionPageResponse>($"pages/{pageId}", cancellationToken);
        var blocksTask = _httpClient.GetFromJsonAsync<NotionBlocksResponse>($"blocks/{pageId}/children", cancellationToken);

        await Task.WhenAll(pageTask, blocksTask);

        var page = await pageTask;
        var blocks = await blocksTask;

        if (page is null) return null;

        var transcriptBody = ExtractTranscriptBody(blocks?.Results ?? []);
        return MapToDetail(page, transcriptBody);
    }

    public async Task UpdateTranscriptAsync(
        string pageId, UpdateTranscriptAccessRequest request, CancellationToken cancellationToken = default)
    {
        var properties = BuildUpdateProperties(request);
        if (properties.Count == 0) return;

        var body = new { properties };
        var response = await _httpClient.PatchAsJsonAsync($"pages/{pageId}", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private NotionTranscriptListItemAccessModel MapToListItem(NotionPageResponse page)
    {
        var p = page.Properties;
        return new NotionTranscriptListItemAccessModel(
            PageId: page.Id,
            RecordingName: GetTitle(p, "Recording Name") ?? "(Untitled)",
            Company: GetText(p, "Company"),
            RepName: GetText(p, "Rep Name"),
            DealStage: GetSelect(p, "Deal Stage"),
            CallType: GetSelect(p, "Call Type"),
            Audience: GetSelect(p, "Audience"),
            DurationMins: GetNumber(p, "Duration (mins)"),
            CreatedAt: GetDate(p, "Created At") ?? page.CreatedTime,
            Reviewed: GetCheckbox(p, "Reviewed")
        );
    }

    private NotionTranscriptAccessModel MapToDetail(NotionPageResponse page, string transcriptBody)
    {
        var p = page.Properties;
        return new NotionTranscriptAccessModel(
            PageId: page.Id,
            RecordingName: GetTitle(p, "Recording Name") ?? "(Untitled)",
            OpportunityId: GetText(p, "Opportunity ID"),
            Company: GetText(p, "Company"),
            ContactName: GetText(p, "Contact Name"),
            ContactTitle: GetText(p, "Contact Title"),
            RepName: GetText(p, "Rep Name"),
            DealStage: GetSelect(p, "Deal Stage"),
            CallType: GetSelect(p, "Call Type"),
            Outcome: GetSelect(p, "Outcome"),
            Audience: GetSelect(p, "Audience"),
            RecordingId: GetText(p, "Recording ID"),
            Speakers: GetText(p, "Speakers"),
            DeviceSerial: GetText(p, "Device Serial"),
            DurationMins: GetNumber(p, "Duration (mins)"),
            CreatedAt: GetDate(p, "Created At") ?? page.CreatedTime,
            Reviewed: GetCheckbox(p, "Reviewed"),
            KeyTopics: GetMultiSelect(p, "Key Topics"),
            ObjectionsRaised: GetText(p, "Objections Raised"),
            CoachNotes: GetText(p, "Coach Notes"),
            CallQuality: GetSelect(p, "Call Quality"),
            CoachableMoments: GetCheckbox(p, "Coachable Moments"),
            SyncedByName: GetCreatedBy(p, "Synced By"),
            TranscriptBody: transcriptBody
        );
    }

    private static List<object> BuildFilters(GetTranscriptsAccessRequest request)
    {
        var filters = new List<object>();
        if (request.RepName is not null)
            filters.Add(new { property = "Rep Name", rich_text = new { contains = request.RepName } });
        if (request.DealStage is not null)
            filters.Add(new { property = "Deal Stage", select = new { equals = request.DealStage } });
        if (request.Reviewed is not null)
            filters.Add(new { property = "Reviewed", checkbox = new { equals = request.Reviewed } });
        return filters;
    }

    private static Dictionary<string, object> BuildUpdateProperties(UpdateTranscriptAccessRequest request)
    {
        var props = new Dictionary<string, object>();
        if (request.OpportunityId is not null) props["Opportunity ID"] = RichText(request.OpportunityId);
        if (request.Company is not null) props["Company"] = RichText(request.Company);
        if (request.ContactName is not null) props["Contact Name"] = RichText(request.ContactName);
        if (request.ContactTitle is not null) props["Contact Title"] = RichText(request.ContactTitle);
        if (request.RepName is not null) props["Rep Name"] = RichText(request.RepName);
        if (request.DealStage is not null) props["Deal Stage"] = Select(request.DealStage);
        if (request.Outcome is not null) props["Outcome"] = Select(request.Outcome);
        if (request.CoachNotes is not null) props["Coach Notes"] = RichText(request.CoachNotes);
        if (request.CallQuality is not null) props["Call Quality"] = Select(request.CallQuality);
        if (request.CoachableMoments is not null) props["Coachable Moments"] = new { checkbox = request.CoachableMoments };
        if (request.Reviewed is not null) props["Reviewed"] = new { checkbox = request.Reviewed };
        return props;
    }

    private static string ExtractTranscriptBody(List<NotionBlockResponse> blocks)
    {
        var lines = blocks.Select(b => b.Type switch
        {
            "paragraph" => string.Concat(b.Paragraph?.RichText.Select(r => r.PlainText) ?? []),
            "heading_1" => string.Concat(b.Heading1?.RichText.Select(r => r.PlainText) ?? []),
            "heading_2" => string.Concat(b.Heading2?.RichText.Select(r => r.PlainText) ?? []),
            "heading_3" => string.Concat(b.Heading3?.RichText.Select(r => r.PlainText) ?? []),
            _ => null
        }).Where(l => l is not null);
        return string.Join("\n", lines);
    }

    private static object[] DefaultSorts() =>
        [new { timestamp = "created_time", direction = "descending" }];

    private static object RichText(string value) =>
        new { rich_text = new[] { new { text = new { content = value } } } };

    private static object Select(string value) =>
        new { select = new { name = value } };

    // --- Property extraction helpers ---

    private static string? GetTitle(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return null;
        if (el.TryGetProperty("title", out var t) && t.GetArrayLength() > 0)
            return t[0].GetProperty("plain_text").GetString();
        return null;
    }

    private static string? GetText(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return null;
        if (el.TryGetProperty("rich_text", out var rt) && rt.GetArrayLength() > 0)
            return rt[0].GetProperty("plain_text").GetString();
        return null;
    }

    private static string? GetSelect(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return null;
        if (el.TryGetProperty("select", out var s) && s.ValueKind != JsonValueKind.Null)
            return s.GetProperty("name").GetString();
        return null;
    }

    private static string[] GetMultiSelect(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return [];
        if (el.TryGetProperty("multi_select", out var ms))
            return ms.EnumerateArray()
                .Select(x => x.GetProperty("name").GetString())
                .Where(x => x is not null)
                .Select(x => x!)
                .ToArray();
        return [];
    }

    private static bool GetCheckbox(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return false;
        return el.TryGetProperty("checkbox", out var c) && c.GetBoolean();
    }

    private static DateTime? GetDate(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return null;
        if (el.TryGetProperty("date", out var d) && d.ValueKind != JsonValueKind.Null)
            if (d.TryGetProperty("start", out var start) && start.ValueKind != JsonValueKind.Null)
                return DateTime.Parse(start.GetString()!);
        return null;
    }

    private static double? GetNumber(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return null;
        if (el.TryGetProperty("number", out var n) && n.ValueKind == JsonValueKind.Number)
            return n.GetDouble();
        return null;
    }

    private static string? GetCreatedBy(Dictionary<string, JsonElement> p, string name)
    {
        if (!p.TryGetValue(name, out var el)) return null;
        if (el.TryGetProperty("created_by", out var cb) && cb.ValueKind != JsonValueKind.Null)
            if (cb.TryGetProperty("name", out var n))
                return n.GetString();
        return null;
    }
}
