using Microsoft.EntityFrameworkCore;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MijnMigraine.Web.Data;
using MijnMigraine.Web.Entities;

namespace MijnMigraine.Web.Helpers;

public class LogicHelper : ILogicHelper
{
    private readonly MijnMigraineDbContext _dbContext;

    public LogicHelper(MijnMigraineDbContext dbContext)
    {
        _dbContext = dbContext;
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
}