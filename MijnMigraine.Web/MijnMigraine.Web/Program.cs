using System.Globalization;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MijnMigraine.Web.Components;
using MijnMigraine.Web.Data;
using MijnMigraine.Web.Llm;
using MudBlazor.Services;
using System.Net;

var culture = new CultureInfo("nl-BE");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    var certificateFilename = context.Configuration.GetValue<string>("CERTIFICATE_FILENAME");
    var certificatePassword = context.Configuration.GetValue<string>("CERTIFICATE_PASSWORD");
    var port = context.Configuration.GetValue<int?>("PORT") ?? 8080;

    if (certificateFilename == null)
    {
        options.Listen(IPAddress.Any, port);
    }
    else
    {
        options.Listen(IPAddress.Any, port, listenOption => listenOption.UseHttps(certificateFilename, certificatePassword));
    }
});

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.Configure<LlmAnalysisOptions>(options =>
{
    options.Endpoint = builder.Configuration.GetValue<string>("LLM_ANALYSIS_ENDPOINT") ?? string.Empty;
    options.ApiKey = builder.Configuration.GetValue<string>("LLM_ANALYSIS_API_KEY") ?? string.Empty;
    options.Model = builder.Configuration.GetValue<string>("LLM_ANALYSIS_MODEL") ?? "gpt-4o-mini";
});
builder.Services.AddScoped<ILlmAnalysisService, LlmAnalysisService>();
builder.Services.AddDbContext<MijnMigraineDbContext>();
builder.Services.AddScoped<ILogicHelper, MijnMigraine.Web.Helpers.LogicHelper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MijnMigraine.Web.Client._Imports).Assembly);

app.MapGet("/api/entries", async (ILogicHelper helper) =>
{
    return await helper.GetEntriesAsync();
})
.WithName("GetAllMigraineEntries");

app.MapPost("/api/entries", async (ILogicHelper helper, MigraineEntryDto entry) =>
{
    return await helper.CreateEntry(entry);
})
.WithName("CreateMigraineEntry");

app.MapPost("/api/entries/analysis", async (ILogicHelper helper, MigraineAnalysisRequestDto request, CancellationToken cancellationToken) =>
{
    return await helper.GetEntriesAnalysisAsync(request.Entries, cancellationToken);
})
.WithName("GetMigraineEntriesAnalysis");

app.Run();