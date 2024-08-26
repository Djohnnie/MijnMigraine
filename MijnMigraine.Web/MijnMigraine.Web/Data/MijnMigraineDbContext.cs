using Microsoft.EntityFrameworkCore;
using MijnMigraine.Web.Entities;

namespace MijnMigraine.Web.Data;

public class MijnMigraineDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DbSet<MigraineEntry> MigraineEntries { get; set; }


    public MijnMigraineDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // $env:CONNECTION_STRING='...'
        optionsBuilder.UseSqlServer(_configuration.GetValue<string>("CONNECTIONSTRING"));

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MigraineEntry>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);
            entity.Property(e => e.SysId).UseIdentityColumn().ValueGeneratedOnAdd();
            entity.HasKey(e => e.SysId).IsClustered();
            entity.Property(e => e.DateOfOccurrence).IsRequired();
            entity.Property(e => e.Severity).IsRequired();
            entity.Property(e => e.Duration).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}