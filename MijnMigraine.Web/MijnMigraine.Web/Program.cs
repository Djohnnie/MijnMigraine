using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MijnMigraine.Web.Components;
using MijnMigraine.Web.Data;
using MudBlazor.Services;
using System.Net;

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

app.Run();