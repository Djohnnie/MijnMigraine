using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MijnMigraine.Web.Client.Helpers;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ILogicHelper, LogicHelper>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();