namespace NL2Geo.Llm;

public sealed record LlmConfig(
    string Provider,
    string Model,
    string ApiKey,
    double Temperature,
    string OllamaBaseUrl
)
{
    public static LlmConfig FromSettings(Config.PluginSettings settings)
        => new(
            settings.LlmProvider,
            settings.LlmModel,
            settings.ApiKey,
            settings.Temperature,
            settings.OllamaBaseUrl
        );
}
