namespace NL2Geo.Config;

public sealed class PluginSettings
{
    public string LlmProvider { get; init; } = GetString("NL2GEO_LLM_PROVIDER", "openai");
    public string LlmModel { get; init; } = GetString("NL2GEO_LLM_MODEL", "gpt-4o-mini");
    public string ApiKey { get; init; } = GetString("NL2GEO_API_KEY", string.Empty);
    public string OllamaBaseUrl { get; init; } = GetString("NL2GEO_OLLAMA_BASE_URL", "http://localhost:11434");
    public double Temperature { get; init; } = GetDouble("NL2GEO_LLM_TEMPERATURE", 0.2);
    public bool UseDeterministicFallback { get; init; } = true;
    public bool EnablePreview { get; init; } = true;

    private static string GetString(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static double GetDouble(string key, double fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return double.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
