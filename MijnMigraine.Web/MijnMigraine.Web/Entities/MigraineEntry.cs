namespace MijnMigraine.Web.Entities;

public class MigraineEntry
{
    public Guid Id { get; set; }
    public int SysId { get; set; }
    public DateTime DateOfOccurrence { get; set; }
    public byte Severity { get; set; }
    public decimal Duration { get; set; }
    public string? AdditionalInfo { get; set; }
}