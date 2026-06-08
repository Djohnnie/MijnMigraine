using Microsoft.EntityFrameworkCore;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MijnMigraine.Web.Llm;
using MijnMigraine.Web.Data;
using MijnMigraine.Web.Entities;

namespace MijnMigraine.Web.Helpers;

public class LogicHelper : ILogicHelper
{
    private readonly MijnMigraineDbContext _dbContext;
    private readonly ILlmAnalysisService _llmAnalysisService;

    public LogicHelper(MijnMigraineDbContext dbContext, ILlmAnalysisService llmAnalysisService)
    {
        _dbContext = dbContext;
        _llmAnalysisService = llmAnalysisService;
    }

    public async Task<List<MigraineEntryDto>> GetEntriesAsync()
    {
        var entries = await _dbContext.MigraineEntries.OrderByDescending(x => x.DateOfOccurrence).ToListAsync();

        return entries.Select(x => new MigraineEntryDto(x.DateOfOccurrence, x.Severity, x.Duration, x.AdditionalInfo)).ToList();
    }

    public async Task<List<MigraineEntryDto>> CreateEntry(MigraineEntryDto entry)
    {
        var newEntry = new MigraineEntry
        {
            Id = Guid.NewGuid(),
            DateOfOccurrence = entry.DateOfOccurrence,
            Severity = (byte)entry.Severity,
            Duration = entry.Duration,
            AdditionalInfo = entry.AdditionalInfo
        };

        _dbContext.MigraineEntries.Add(newEntry);
        await _dbContext.SaveChangesAsync();

        return await GetEntriesAsync();
    }

    public async Task<MigraineAnalysisResponseDto> GetEntriesAnalysisAsync(IReadOnlyCollection<MigraineEntryDto> entries, CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
        {
            return new MigraineAnalysisResponseDto("Er zijn geen registraties om te analyseren.");
        }

        var entriesForPrompt = entries
            .OrderByDescending(x => x.DateOfOccurrence)
            .Take(MigraineAnalysisDefaults.MaxEntries)
            .ToList();

        var analysis = await _llmAnalysisService.GenerateAnalysisAsync(entriesForPrompt, cancellationToken);
        return new MigraineAnalysisResponseDto(analysis);
    }
}