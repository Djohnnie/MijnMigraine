namespace MijnMigraine.Web.Llm;

public sealed class LlmAnalysisOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}
