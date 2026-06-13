namespace LC.Access.NetSuite.Configuration;

public class NetSuiteConfiguration
{
    public string AccountId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string CertId { get; set; } = "";
    public string PrivateKeyPem { get; set; } = "";

    public string TokenEndpoint =>
        $"https://{AccountId.Replace('_', '-').Trim().ToLowerInvariant()}.suitetalk.api.netsuite.com/services/rest/auth/oauth2/v1/token";

    public string RestBaseUrl =>
        $"https://{AccountId.Replace('_', '-').Trim().ToLowerInvariant()}.suitetalk.api.netsuite.com/services/rest/record/v1/";
}
