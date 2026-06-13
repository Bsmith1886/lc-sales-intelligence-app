using LC.Access.NetSuite;
using LC.Access.NetSuite.Configuration;
using LC.Access.Notion;
using LC.Access.Notion.Configuration;
using LC.Host.Api;
using LC.Host.Common;
using LC.Manager.Transcript;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.Configure<NotionConfiguration>(builder.Configuration.GetSection("NotionConfiguration"));
builder.Services.Configure<NetSuiteConfiguration>(builder.Configuration.GetSection("NetSuiteConfiguration"));
LC.Access.Notion.ServiceInjection.ConfigureServices(builder.Services);
LC.Access.NetSuite.ServiceInjection.ConfigureServices(builder.Services);
LC.Manager.Transcript.ServiceInjection.ConfigureServices(builder.Services);
builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
