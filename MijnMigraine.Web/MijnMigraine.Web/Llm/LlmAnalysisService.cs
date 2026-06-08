using System.Text;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.Extensions.Options;
using MijnMigraine.Web.Client.Contracts;

namespace MijnMigraine.Web.Llm;

public sealed class LlmAnalysisService : ILlmAnalysisService
{
    private readonly LlmAnalysisOptions _options;

    public LlmAnalysisService(IOptions<LlmAnalysisOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> GenerateAnalysisAsync(IReadOnlyCollection<MigraineEntryDto> entries, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return "LLM-analyse is nog niet geconfigureerd. Stel LLM_ANALYSIS_ENDPOINT en LLM_ANALYSIS_API_KEY in als omgevingsvariabelen.";
        }

        var endpoint = new Uri(_options.Endpoint);
        var client = new AzureOpenAIClient(endpoint, new ApiKeyCredential(_options.ApiKey));
        var chatClient = client.GetChatClient(_options.Model);
        var agentClient = chatClient.AsAIAgent(
            name: "MigraineAnalyseAgent",
            description: "Analyseert migraine-registraties, benoemt patronen en mitigaties in het Nederlands, en geeft geen advies over toekomstige doktersafspraken.");

        var prompt = BuildPrompt(entries);
        var responseBuilder = new StringBuilder();

        await foreach (var response in agentClient.RunStreamingAsync(prompt).WithCancellation(cancellationToken))
        {
            if (!string.IsNullOrEmpty(response.Text))
            {
                responseBuilder.Append(response.Text);
            }
        }

        var analysis = responseBuilder.ToString().Trim();
        if (string.IsNullOrWhiteSpace(analysis))
        {
            throw new InvalidOperationException("Agent Framework gaf geen analyse terug.");
        }

        return analysis;
    }

    private static string BuildPrompt(IReadOnlyCollection<MigraineEntryDto> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Je bent een medische assistent voor migraine-logboekanalyse.");
        sb.AppendLine("Geef geen diagnose.");
        sb.AppendLine("Geef geen advies over toekomstige doktersafspraken, doorverwijzingen, consultaties of behandeling bij artsen.");
        sb.AppendLine("Focus uitsluitend op patronen in de logs en praktische mitigaties die ik zelf kan toepassen.");
        sb.AppendLine("Antwoord uitsluitend in geldige Markdown.");
        sb.AppendLine("Gebruik duidelijke sectiekoppen en zet elk lijstitem op een eigen regel met Markdown-bullets of genummerde lijsten.");
        sb.AppendLine("Gebruik geen HTML en geen codeblokken.");
        sb.AppendLine();
        sb.AppendLine("Analyseer deze migraine-registraties:");

        foreach (var entry in entries.OrderByDescending(x => x.DateOfOccurrence))
        {
            sb.Append("- ");
            sb.Append(entry.DateOfOccurrence.ToString("yyyy-MM-dd HH:mm"));
            sb.Append(" | ernst ");
            sb.Append(entry.Severity);
            sb.Append("/10");
            sb.Append(" | duur ");
            sb.Append(entry.Duration.ToString("0.##"));
            sb.Append("u");
            sb.Append(" | opmerking: ");
            sb.AppendLine(string.IsNullOrWhiteSpace(entry.AdditionalInfo) ? "geen" : entry.AdditionalInfo);
        }

        sb.AppendLine();
        sb.AppendLine("Formaat:");
        sb.AppendLine("## Samenvatting");
        sb.AppendLine("2-4 zinnen.");
        sb.AppendLine();
        sb.AppendLine("## Patronen");
        sb.AppendLine("- Gebruik een Markdown-bulleted list.");
        sb.AppendLine();
        sb.AppendLine("## Mitigaties");
        sb.AppendLine("- Gebruik een Markdown-bulleted list met concrete acties die ik zelf kan proberen.");

        return sb.ToString();
    }
}
