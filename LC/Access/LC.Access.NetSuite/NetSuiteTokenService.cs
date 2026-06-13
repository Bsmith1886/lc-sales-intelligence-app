using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using LC.Access.NetSuite.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LC.Access.NetSuite;

public class NetSuiteTokenService
{
    private const string TokenScope = "rest_webservices";
    private const string ClientAssertionType =
        "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

    private readonly ILogger<NetSuiteTokenService> _logger;
    private readonly NetSuiteConfiguration _config;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry;

    public NetSuiteTokenService(IOptions<NetSuiteConfiguration> config, ILogger<NetSuiteTokenService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _cachedToken;

            using var client = new HttpClient();
            var assertion = SignAssertion(_config.TokenEndpoint);

            using var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_assertion_type", ClientAssertionType),
                    new KeyValuePair<string, string>("client_assertion", assertion),
                }),
            };

            _logger.LogInformation("Acquiring NetSuite token for account {AccountId}", _config.AccountId);
            using var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(
                    $"NetSuite token request failed ({(int)response.StatusCode} {response.StatusCode}): {body}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
                ?? throw new InvalidOperationException("NetSuite returned an empty token response.");

            _cachedToken = tokenResponse.AccessToken
                ?? throw new InvalidOperationException("NetSuite token response missing access_token.");

            var lifetimeSeconds = tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600;
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(lifetimeSeconds - 300);

            _logger.LogInformation("NetSuite token acquired, expires in {ExpiresIn}s", tokenResponse.ExpiresIn);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateToken()
    {
        _cachedToken = null;
        _tokenExpiry = DateTimeOffset.MinValue;
    }

    public string BuildAssertionForDebug(string audience) => SignAssertion(audience);

    private string SignAssertion(string audience)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_config.PrivateKeyPem);

        var key = new RsaSecurityKey(rsa)
        {
            KeyId = _config.CertId,
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
        };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSsaPssSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config.ClientId,
            Audience = audience,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(1),
            Claims = new Dictionary<string, object>
            {
                ["scope"] = new[] { TokenScope },
                ["jti"] = Guid.NewGuid().ToString(),
            },
            SigningCredentials = credentials,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
