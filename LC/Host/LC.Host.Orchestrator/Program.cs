using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<ApiProject>("api");

builder.AddProject<ProxyProject>("proxy")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();

class ApiProject : IProjectMetadata
{
    public string ProjectPath => "../LC.Host.Api/LC.Host.Api.csproj";
}

class ProxyProject : IProjectMetadata
{
    public string ProjectPath => "../LC.Host.Proxy/LC.Host.Proxy.csproj";
}

