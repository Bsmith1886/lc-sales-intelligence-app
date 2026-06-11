using LC.Access.Notion;
using LC.Access.Notion.Configuration;
using LC.Host.Common;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.Configure<NotionConfiguration>(builder.Configuration.GetSection("NotionConfiguration"));
LC.Access.Notion.ServiceInjection.ConfigureServices(builder.Services);
builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
