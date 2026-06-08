using Microsoft.AspNetCore.Components;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MudBlazor;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace MijnMigraine.Web.Client.Pages;

public partial class Home : ComponentBase, IDisposable
{
    private readonly ILogicHelper _logicHelper;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _analysisCancellationSource;

    protected List<MigraineEntryDto>? Entries { get; private set; }
    protected int? SelectedYear { get; set; }
    protected string? AnalysisText { get; set; }
    protected string? AnalysisError { get; set; }
    protected bool IsAnalysisLoading { get; set; }
    protected bool IsAnalysisExpanded { get; set; } = true;
    protected MarkupString AnalysisMarkup => new(RenderAnalysisHtml());

    protected void ToggleAnalysis() => IsAnalysisExpanded = !IsAnalysisExpanded;

    protected string AnalysisSubtitle
    {
        get
        {
            var total = FilteredEntries.Count;
            var yearSuffix = SelectedYear.HasValue ? $" in {SelectedYear}" : string.Empty;

            if (total == 0)
            {
                return $"Geen registraties om te analyseren{yearSuffix}";
            }

            if (total > MigraineAnalysisDefaults.MaxEntries)
            {
                return $"Op basis van de laatste {MigraineAnalysisDefaults.MaxEntries} van {total} registraties{yearSuffix}";
            }

            return $"Op basis van {total} registratie{(total == 1 ? "" : "s")}{yearSuffix}";
        }
    }

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
        StartAnalysisRefresh();
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
        var entries = Entries ?? [];
        var oldest = entries.Min(e => e.DateOfOccurrence);
        var totalDays = (DateTime.Now - oldest).TotalDays;
        if (totalDays <= 0) return (entries.Count, "per week");

        var totalWeeks = totalDays / 7.0;
        var perWeek = entries.Count / totalWeeks;
        if (perWeek >= 1) return (perWeek, "per week");

        var totalMonths = totalDays / 30.44;
        var perMonth = entries.Count / totalMonths;
        if (perMonth >= 1) return (perMonth, "per maand");

        var totalYears = totalDays / 365.25;
        var perYear = entries.Count / totalYears;
        return (perYear, "per jaar");
    }

    protected Task OnYearFilterChanged(int? year)
    {
        SelectedYear = year;
        StartAnalysisRefresh();
        return Task.CompletedTask;
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
            StartAnalysisRefresh();
            StateHasChanged();
        }
    }

    private void StartAnalysisRefresh()
    {
        _ = RefreshAnalysisAsync();
    }

    private async Task RefreshAnalysisAsync()
    {
        _analysisCancellationSource?.Cancel();
        _analysisCancellationSource?.Dispose();
        _analysisCancellationSource = new CancellationTokenSource();

        AnalysisError = null;
        IsAnalysisLoading = true;

        try
        {
            var analysis = await _logicHelper.GetEntriesAnalysisAsync(FilteredEntries, _analysisCancellationSource.Token);
            AnalysisText = analysis.Analysis;
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (HttpRequestException)
        {
            AnalysisText = null;
            AnalysisError = "Er was een netwerkprobleem bij het ophalen van de LLM-analyse.";
        }
        catch (InvalidOperationException exception)
        {
            AnalysisText = null;
            AnalysisError = exception.Message;
        }
        catch (Exception exception)
        {
            AnalysisText = null;
            AnalysisError = $"Er ging iets mis bij het ophalen van de LLM-analyse: {exception.Message}";
        }
        finally
        {
            IsAnalysisLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _analysisCancellationSource?.Cancel();
        _analysisCancellationSource?.Dispose();
    }

    private string RenderAnalysisHtml()
    {
        if (string.IsNullOrWhiteSpace(AnalysisText))
        {
            return string.Empty;
        }

        return RenderMarkdownLikeHtml(NormalizeMarkdownSource(AnalysisText));
    }

    private static string NormalizeMarkdownSource(string markdown)
    {
        var normalized = markdown.Trim();

        if (normalized.StartsWith('"') && normalized.EndsWith('"') && normalized.Length >= 2)
        {
            normalized = normalized[1..^1];
        }

        if (normalized.Contains("\\n", StringComparison.Ordinal) &&
            !normalized.Contains(Environment.NewLine, StringComparison.Ordinal) &&
            !normalized.Contains('\n'))
        {
            if (TryDeserializeJsonString(normalized, out var deserialized))
            {
                normalized = deserialized;
            }
            else
            {
                normalized = normalized
                    .Replace("\\r\\n", Environment.NewLine, StringComparison.Ordinal)
                    .Replace("\\n", Environment.NewLine, StringComparison.Ordinal)
                    .Replace("\\t", "\t", StringComparison.Ordinal);
            }
        }

        normalized = normalized.Replace("\r\n", "\n", StringComparison.Ordinal);
        normalized = Regex.Replace(normalized, @"[ \t]+(?=#{1,6}\s)", "\n");
        normalized = Regex.Replace(normalized, @"[ \t]+(?=(?:[-*]|\d+\.)\s)", "\n");

        return normalized;
    }

    private static bool TryDeserializeJsonString(string value, out string result)
    {
        try
        {
            var json = value.StartsWith('"') && value.EndsWith('"')
                ? value
                : $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

            result = JsonSerializer.Deserialize<string>(json) ?? string.Empty;
            return true;
        }
        catch (JsonException)
        {
            result = string.Empty;
            return false;
        }
    }

    private static string RenderMarkdownLikeHtml(string markdown)
    {
        var html = new StringBuilder();
        var paragraphLines = new List<string>();
        var listType = ListType.None;

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                FlushParagraph(html, paragraphLines);
                CloseList(html, ref listType);
                continue;
            }

            var headingMatch = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
            if (headingMatch.Success)
            {
                FlushParagraph(html, paragraphLines);
                CloseList(html, ref listType);

                var level = Math.Min(4, headingMatch.Groups[1].Value.Length);
                html.Append('<').Append('h').Append(level).Append('>');
                html.Append(RenderInlineMarkdown(headingMatch.Groups[2].Value));
                html.Append("</h").Append(level).Append('>');
                continue;
            }

            var bulletMatch = Regex.Match(line, @"^[-*]\s+(.+)$");
            if (bulletMatch.Success)
            {
                FlushParagraph(html, paragraphLines);
                EnsureList(html, ref listType, ListType.Unordered);
                html.Append("<li>").Append(RenderInlineMarkdown(bulletMatch.Groups[1].Value)).Append("</li>");
                continue;
            }

            var numberedMatch = Regex.Match(line, @"^\d+\.\s+(.+)$");
            if (numberedMatch.Success)
            {
                FlushParagraph(html, paragraphLines);
                EnsureList(html, ref listType, ListType.Ordered);
                html.Append("<li>").Append(RenderInlineMarkdown(numberedMatch.Groups[1].Value)).Append("</li>");
                continue;
            }

            CloseList(html, ref listType);
            paragraphLines.Add(line);
        }

        FlushParagraph(html, paragraphLines);
        CloseList(html, ref listType);

        return html.ToString();
    }

    private static void FlushParagraph(StringBuilder html, List<string> paragraphLines)
    {
        if (paragraphLines.Count == 0)
        {
            return;
        }

        html.Append("<p>")
            .Append(RenderInlineMarkdown(string.Join(" ", paragraphLines)))
            .Append("</p>");

        paragraphLines.Clear();
    }

    private static void EnsureList(StringBuilder html, ref ListType currentListType, ListType requestedListType)
    {
        if (currentListType == requestedListType)
        {
            return;
        }

        CloseList(html, ref currentListType);
        html.Append(requestedListType == ListType.Ordered ? "<ol>" : "<ul>");
        currentListType = requestedListType;
    }

    private static void CloseList(StringBuilder html, ref ListType currentListType)
    {
        if (currentListType == ListType.None)
        {
            return;
        }

        html.Append(currentListType == ListType.Ordered ? "</ol>" : "</ul>");
        currentListType = ListType.None;
    }

    private static string RenderInlineMarkdown(string text)
    {
        var encoded = WebUtility.HtmlEncode(text.Trim());

        encoded = Regex.Replace(encoded, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        encoded = Regex.Replace(encoded, @"__(.+?)__", "<strong>$1</strong>");
        encoded = Regex.Replace(encoded, @"\*(.+?)\*", "<em>$1</em>");
        encoded = Regex.Replace(encoded, @"_(.+?)_", "<em>$1</em>");
        encoded = Regex.Replace(encoded, @"`(.+?)`", "<code>$1</code>");

        return encoded;
    }

    private enum ListType
    {
        None,
        Unordered,
        Ordered
    }
}