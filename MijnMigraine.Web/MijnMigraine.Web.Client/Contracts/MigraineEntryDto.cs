namespace MijnMigraine.Web.Client.Contracts;

public record MigraineEntryDto(DateTime DateOfOccurrence, int Severity, decimal Duration, string? AdditionalInfo);