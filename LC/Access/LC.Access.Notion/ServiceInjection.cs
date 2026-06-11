using System.Net.Http.Headers;
using LC.Access.Notion.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LC.Access.Notion;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient("Notion", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<NotionConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.ApiToken);
            client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");
        });

        services.AddScoped<INotionTranscriptAccessor, NotionTranscriptAccessor>();
    }
}
