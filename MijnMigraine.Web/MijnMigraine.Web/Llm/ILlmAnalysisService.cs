using MijnMigraine.Web.Client.Contracts;

namespace MijnMigraine.Web.Llm;

public interface ILlmAnalysisService
{
    Task<string> GenerateAnalysisAsync(IReadOnlyCollection<MigraineEntryDto> entries, CancellationToken cancellationToken = default);
}
