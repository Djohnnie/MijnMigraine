using Microsoft.AspNetCore.Components;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;

namespace MijnMigraine.Web.Client.Pages;

public partial class Home : ComponentBase
{
    private readonly ILogicHelper _logicHelper;

    protected List<MigraineEntryDto> Entries { get; private set; }

    protected DateTime? DateOfOccurrence { get; set; }
    protected TimeSpan? TimeOfOccurrence { get; set; }
    protected int Severity { get; set; }
    protected decimal? Duration { get; set; }
    protected string? AdditionalInfo { get; set; }

    public Home(ILogicHelper logicHelper)
    {
        _logicHelper = logicHelper;
    }

    protected override async Task OnInitializedAsync()
    {
        Entries = await _logicHelper.GetEntriesAsync();
    }

    protected void OnCreateMigraineEntryCommand()
    {
        var occurrence = new DateTime(DateOfOccurrence.Value.Year, DateOfOccurrence.Value.Month, DateOfOccurrence.Value.Day, TimeOfOccurrence.Value.Hours, TimeOfOccurrence.Value.Minutes, 0);
        var entry = new MigraineEntryDto(occurrence, Severity, Duration ?? 0, AdditionalInfo);

        Entries.Add(entry);

        StateHasChanged();
    }
}