namespace MijnMigraine.Web.Client.Contracts;

public record MigraineEntryDto(DateTime DateOfOccurrence, byte Severity, decimal Duration, string? AdditionalInfo);