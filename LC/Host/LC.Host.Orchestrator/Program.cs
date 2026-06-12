using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.LC_Host_Api>("api");
builder.AddProject<Projects.LC_Host_Proxy>("proxy").WithReference(api).WaitFor(api);

builder.Build().Run();
