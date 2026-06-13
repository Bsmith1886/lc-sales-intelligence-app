namespace LC.Access.NetSuite;

public class NetSuiteOAuthHandler : DelegatingHandler
{
    private readonly NetSuiteTokenService _tokenService;

    public NetSuiteOAuthHandler(NetSuiteTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokenService.GetTokenAsync(ct);
        request.Headers.Authorization = new("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _tokenService.InvalidateToken();
            token = await _tokenService.GetTokenAsync(ct);
            request.Headers.Authorization = new("Bearer", token);
            response = await base.SendAsync(request, ct);
        }

        return response;
    }
}
