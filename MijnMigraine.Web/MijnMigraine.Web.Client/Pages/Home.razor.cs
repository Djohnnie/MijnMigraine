using Microsoft.AspNetCore.Components;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MudBlazor;

namespace MijnMigraine.Web.Client.Pages;

public partial class Home : ComponentBase
{
    private readonly ILogicHelper _logicHelper;
    private readonly IDialogService _dialogService;

    protected List<MigraineEntryDto> Entries { get; private set; }
    protected int? SelectedYear { get; set; }

    protected List<MigraineEntryDto> FilteredEntries =>
        SelectedYear.HasValue
            ? Entries?.Where(e => e.DateOfOccurrence.Year == SelectedYear.Value).ToList() ?? []
            : Entries ?? [];

    protected IEnumerable<int> AvailableYears =>
        Entries?.Select(e => e.DateOfOccurrence.Year).Distinct().OrderByDescending(y => y) ?? Enumerable.Empty<int>();

    public Home(ILogicHelper logicHelper, IDialogService dialogService)
    {
        _logicHelper = logicHelper;
        _dialogService = dialogService;
    }

    protected override async Task OnInitializedAsync()
    {
        Entries = await _logicHelper.GetEntriesAsync();
    }

    protected int AttacksThisYear =>
        Entries?.Count(e => e.DateOfOccurrence.Year == DateTime.Now.Year) ?? 0;

    protected int AttacksThisMonth =>
        Entries?.Count(e => e.DateOfOccurrence.Year == DateTime.Now.Year
                         && e.DateOfOccurrence.Month == DateTime.Now.Month) ?? 0;

    protected string AverageFrequencyValue
    {
        get
        {
            if (Entries == null || Entries.Count < 2) return "—";
            var (value, _) = CalculateFrequency();
            return value.ToString("0.#");
        }
    }

    protected string AverageFrequencyPeriod
    {
        get
        {
            if (Entries == null || Entries.Count < 2) return "per week";
            var (_, period) = CalculateFrequency();
            return period;
        }
    }

    private (double value, string period) CalculateFrequency()
    {
        var oldest = Entries!.Min(e => e.DateOfOccurrence);
        var totalDays = (DateTime.Now - oldest).TotalDays;
        if (totalDays <= 0) return (Entries.Count, "per week");

        var totalWeeks = totalDays / 7.0;
        var perWeek = Entries.Count / totalWeeks;
        if (perWeek >= 1) return (perWeek, "per week");

        var totalMonths = totalDays / 30.44;
        var perMonth = Entries.Count / totalMonths;
        if (perMonth >= 1) return (perMonth, "per maand");

        var totalYears = totalDays / 365.25;
        var perYear = Entries.Count / totalYears;
        return (perYear, "per jaar");
    }

    protected void OnYearFilterChanged(int? year)
    {
        SelectedYear = year;
    }

    protected async Task OpenCreateDialog()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseOnEscapeKey = true,
            BackdropClick = false
        };

        var dialog = await _dialogService.ShowAsync<CreateEntryDialog>("Nieuwe registratie", options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: MigraineEntryDto entry })
        {
            Entries = await _logicHelper.CreateEntry(entry);
            StateHasChanged();
        }
    }
}