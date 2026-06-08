# MijnMigraine
Small Blazor web app to keep track of migraine episodes

## LLM-analyse configureren
De analysekaart op de homepagina gebruikt de Microsoft Agent Framework Chat-integratie (`AgentFramework.Chat`-stijl) bovenop Azure OpenAI.

De analyse wordt in Markdown gevraagd en in de Blazor UI als opgemaakte inhoud gerenderd, zodat koppen en lijsten correct zichtbaar blijven.

Stel deze omgevingsvariabelen in voor de server:
- `LLM_ANALYSIS_ENDPOINT` (volledige URL naar je chat completions endpoint)
- `LLM_ANALYSIS_API_KEY`
- `LLM_ANALYSIS_MODEL` (optioneel, standaard `gpt-4o-mini`)
