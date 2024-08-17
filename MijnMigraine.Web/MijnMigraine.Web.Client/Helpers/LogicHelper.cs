using MijnMigraine.Web.Client.Contracts;
using System.Net.Http.Json;

namespace MijnMigraine.Web.Client.Helpers;

public interface ILogicHelper
{
    Task<List<MigraineEntryDto>> GetEntriesAsync();

    Task<List<MigraineEntryDto>> CreateEntry(MigraineEntryDto entry);
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
        return await _httpClient.GetFromJsonAsync<List<MigraineEntryDto>>("api/entries");
    }

    public async Task<List<MigraineEntryDto>> CreateEntry(MigraineEntryDto entry)
    {
        var response = await _httpClient.PostAsJsonAsync("api/entries", entry);
        return await response.Content.ReadFromJsonAsync<List<MigraineEntryDto>>();
    }
}