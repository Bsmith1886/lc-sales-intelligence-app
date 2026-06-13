using LC.Access.NetSuite.Models;

namespace LC.Access.NetSuite;

public interface INetSuiteOpportunityAccessor
{
    Task<string> GetOpportunitiesRawAsync(CancellationToken ct = default);
    Task<NetSuiteOpportunityAccessModel?> GetOpportunityAsync(string internalId, CancellationToken ct = default);
}
