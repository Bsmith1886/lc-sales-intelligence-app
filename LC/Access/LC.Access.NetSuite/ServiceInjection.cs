using LC.Access.NetSuite.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LC.Access.NetSuite;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<NetSuiteTokenService>();
        services.AddTransient<NetSuiteOAuthHandler>();

        services.AddHttpClient("NetSuite", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<NetSuiteConfiguration>>().Value;
            client.BaseAddress = new Uri(config.RestBaseUrl);
        }).AddHttpMessageHandler<NetSuiteOAuthHandler>();

        services.AddScoped<INetSuiteOpportunityAccessor, NetSuiteOpportunityAccessor>();
        services.AddScoped<INetSuiteCustomerAccessor, NetSuiteCustomerAccessor>();
    }
}
