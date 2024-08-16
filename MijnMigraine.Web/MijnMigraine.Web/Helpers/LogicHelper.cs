using Microsoft.EntityFrameworkCore;
using MijnMigraine.Web.Client.Contracts;
using MijnMigraine.Web.Client.Helpers;
using MijnMigraine.Web.Data;

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
}