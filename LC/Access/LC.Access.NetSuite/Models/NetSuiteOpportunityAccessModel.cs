namespace LC.Access.NetSuite.Models;

public record NetSuiteOpportunityAccessModel(
    string InternalId,
    string? DocumentNumber,       // tranId
    string? Title,                // title
    string? CustomerCode,         // entity.refName prefix e.g. "CUS354"
    string? CompanyName,          // entity.refName remainder
    string? OpportunityStatus,    // custbody15.refName
    string? SalesRep,             // salesRep.refName
    string? InsideSalesRep,       // custbody2
    decimal? ProjectedTotal,      // projectedTotal
    double? Probability,          // probability
    string? LeadSource,           // leadSource.refName
    string? RNumber,              // custbody43
    string? ProjectType,          // custbody_project_type
    string? NextFollowUp,         // custbody_ledoppnextfollowup
    string? FollowUpPriority,     // custbody_ledoppfollowuppriority
    string? ProjectInternalId,    // job.id
    string? ProjectName,          // job.refName
    string? CloseDate             // closeDate
);
