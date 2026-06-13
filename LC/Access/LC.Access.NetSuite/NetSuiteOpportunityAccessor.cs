using System.Text.Json;
using System.Text.RegularExpressions;
using LC.Access.NetSuite.Models;

namespace LC.Access.NetSuite;

public class NetSuiteOpportunityAccessor : INetSuiteOpportunityAccessor
{
    private readonly HttpClient _httpClient;

    public NetSuiteOpportunityAccessor(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("NetSuite");
    }

    public async Task<string> GetOpportunitiesRawAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("opportunity?limit=5", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<NetSuiteOpportunityAccessModel?> GetOpportunityAsync(string internalId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"opportunity/{internalId}?expandSubResources=true", ct);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var entityRefName = GetRefName(root, "entity");
        var (customerCode, companyName) = ParseEntityRefName(entityRefName);

        return new NetSuiteOpportunityAccessModel(
            InternalId: internalId,
            DocumentNumber: GetString(root, "tranId"),
            Title: GetString(root, "title"),
            CustomerCode: customerCode,
            CompanyName: companyName,
            OpportunityStatus: GetRefName(root, "custbody15"),
            SalesRep: GetRefName(root, "salesRep"),
            InsideSalesRep: GetRefNameOrString(root, "custbody2"),
            ProjectedTotal: GetDecimal(root, "projectedTotal"),
            Probability: GetDouble(root, "probability"),
            LeadSource: GetRefName(root, "leadSource"),
            RNumber: GetRefNameOrString(root, "custbody43"),
            ProjectType: GetRefNameOrString(root, "custbody_project_type"),
            NextFollowUp: GetString(root, "custbody_ledoppnextfollowup"),
            FollowUpPriority: GetRefNameOrString(root, "custbody_ledoppfollowuppriority"),
            ProjectInternalId: GetRefId(root, "job"),
            ProjectName: GetRefName(root, "job"),
            CloseDate: GetString(root, "closeDate")
        );
    }

    private static string? GetRefName(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.Object &&
            prop.TryGetProperty("refName", out var refName) &&
            refName.ValueKind == JsonValueKind.String)
            return refName.GetString();
        return null;
    }

    private static string? GetRefId(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.Object &&
            prop.TryGetProperty("id", out var id) &&
            id.ValueKind == JsonValueKind.String)
            return id.GetString();
        return null;
    }

    private static string? GetRefNameOrString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop)) return null;
        if (prop.ValueKind == JsonValueKind.Object)
        {
            if (prop.TryGetProperty("refName", out var refName) && refName.ValueKind == JsonValueKind.String)
                return refName.GetString();
            return null;
        }
        if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
        return null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString();
        return null;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
            return prop.GetDecimal();
        return null;
    }

    private static double? GetDouble(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
            return prop.GetDouble();
        return null;
    }

    private static (string? customerCode, string? companyName) ParseEntityRefName(string? refName)
    {
        if (string.IsNullOrEmpty(refName)) return (null, null);
        var match = Regex.Match(refName, @"^([A-Z]+\d+)\s+(.+)$");
        if (match.Success) return (match.Groups[1].Value, match.Groups[2].Value);
        return (null, refName);
    }
}
