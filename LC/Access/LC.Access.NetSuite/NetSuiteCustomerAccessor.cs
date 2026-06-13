namespace LC.Access.NetSuite;

public class NetSuiteCustomerAccessor : INetSuiteCustomerAccessor
{
    private readonly HttpClient _httpClient;

    public NetSuiteCustomerAccessor(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("NetSuite");
    }
}
