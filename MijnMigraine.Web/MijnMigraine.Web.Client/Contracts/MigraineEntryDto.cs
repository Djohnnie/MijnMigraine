namespace MijnMigraine.Web.Client.Contracts;

public record MigraineEntryDto(DateTime DateOfOccurrence, int Severity, decimal Duration, string? AdditionalInfo);

public record MigraineAnalysisRequestDto(IReadOnlyCollection<MigraineEntryDto> Entries);

public record MigraineAnalysisResponseDto(string Analysis);

public static class MigraineAnalysisDefaults
{
    public const int MaxEntries = 5;
}