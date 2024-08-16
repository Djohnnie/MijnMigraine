using Microsoft.AspNetCore.Components;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace MijnMigraine.Web.Client.Pages;

public partial class Home : ComponentBase
{
    private readonly ILogicHelper _logicHelper;

    protected List<MigraineEntryDto> Entries { get; private set; }

    public Home(ILogicHelper logicHelper)
    {
        _logicHelper = logicHelper;
    }

    protected override async Task OnInitializedAsync()
    {
        Entries = await _logicHelper.GetEntriesAsync();
    }
}