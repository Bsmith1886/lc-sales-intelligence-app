using Microsoft.Extensions.DependencyInjection;

namespace LC.Manager.Transcript;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITranscriptManager, TranscriptManager>();
    }
}
