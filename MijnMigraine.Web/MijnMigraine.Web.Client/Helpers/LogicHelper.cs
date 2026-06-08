using MijnMigraine.Web.Client.Contracts;
using System.Net.Http.Json;

namespace MijnMigraine.Web.Client.Helpers;

public interface ILogicHelper
{
    Task<List<MigraineEntryDto>> GetEntriesAsync();

    Task<List<MigraineEntryDto>> CreateEntry(MigraineEntryDto entry);

    Task<MigraineAnalysisResponseDto> GetEntriesAnalysisAsync(IReadOnlyCollection<MigraineEntryDto> entries, CancellationToken cancellationToken = default);
}

public class LogicHelper : ILogicHelper
{
    private readonly HttpClient _httpClient;

    public LogicHelper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<MigraineEntryDto>> GetEntriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<MigraineEntryDto>>("api/entries") ?? [];
    }

    public async Task<List<MigraineEntryDto>> CreateEntry(MigraineEntryDto entry)
    {
        var response = await _httpClient.PostAsJsonAsync("api/entries", entry);
        return await response.Content.ReadFromJsonAsync<List<MigraineEntryDto>>() ?? [];
    }

    public async Task<MigraineAnalysisResponseDto> GetEntriesAnalysisAsync(IReadOnlyCollection<MigraineEntryDto> entries, CancellationToken cancellationToken = default)
    {
        var request = new MigraineAnalysisRequestDto(entries);
        var response = await _httpClient.PostAsJsonAsync("api/entries/analysis", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"LLM-analyse kon niet geladen worden ({(int)response.StatusCode}): {errorDetails}");
        }

        var analysis = await response.Content.ReadFromJsonAsync<MigraineAnalysisResponseDto>(cancellationToken);
        if (analysis is null)
        {
            throw new InvalidOperationException("LLM-analyse gaf een leeg antwoord terug.");
        }

        return analysis;
    }
}