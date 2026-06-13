using System.Text;
using System.Text.Json;
using LC.Access.NetSuite;
using LC.Access.NetSuite.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace LC.Host.Api.Controllers;

[ApiController]
[Route("api/netsuite")]
[Authorize]
public class NetSuiteController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public NetSuiteController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [AllowAnonymous]
    [HttpGet("opportunities/{id}")]
    public async Task<IActionResult> GetOpportunity(
        string id,
        [FromServices] INetSuiteOpportunityAccessor opportunityAccessor,
        CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();
        var result = await opportunityAccessor.GetOpportunityAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("opportunities")]
    public async Task<IActionResult> GetOpportunities(
        [FromServices] INetSuiteOpportunityAccessor opportunityAccessor,
        CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();
        var raw = await opportunityAccessor.GetOpportunitiesRawAsync(ct);
        return Content(raw, "application/json");
    }

    [AllowAnonymous]
    [HttpGet("health")]
    public async Task<IActionResult> Health([FromServices] NetSuiteTokenService tokenService, CancellationToken ct)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        try
        {
            await tokenService.GetTokenAsync(ct);
            return Ok(new { tokenAcquired = true });
        }
        catch (Exception ex)
        {
            return Ok(new { tokenAcquired = false, error = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("debug-jwt")]
    public IActionResult DebugJwt(
        [FromServices] NetSuiteTokenService tokenService,
        [FromServices] IOptions<NetSuiteConfiguration> options)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var config = options.Value;
        var jwt = tokenService.BuildAssertionForDebug(config.TokenEndpoint);
        var handler = new JsonWebTokenHandler();
        var token = handler.ReadJsonWebToken(jwt);

        return Ok(new
        {
            tokenEndpoint = config.TokenEndpoint,
            accountId = config.AccountId,
            clientId = config.ClientId,
            certId = config.CertId,
            privateKeyPemLength = config.PrivateKeyPem?.Length,
            privateKeyPemStart = config.PrivateKeyPem?[..Math.Min(30, config.PrivateKeyPem?.Length ?? 0)],
            jwt = new
            {
                header = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(Base64UrlDecode(jwt.Split('.')[0]))),
                payload = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(Base64UrlDecode(jwt.Split('.')[1]))),
            }
        });
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '='));
    }
}
