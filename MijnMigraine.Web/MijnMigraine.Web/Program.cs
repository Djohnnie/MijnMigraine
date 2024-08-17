using Microsoft.EntityFrameworkCore;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MijnMigraine.Web.Client.Pages;
using MijnMigraine.Web.Components;
using MijnMigraine.Web.Data;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

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

app.UseHttpsRedirection();

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