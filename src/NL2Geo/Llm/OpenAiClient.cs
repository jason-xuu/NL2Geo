namespace NL2Geo.Llm;

public sealed class OpenAiClient : ILlmClient
{
    private readonly LlmConfig _config;
    public string Name => "openai";
    public string? LastError { get; private set; }

    public OpenAiClient(LlmConfig config)
    {
        _config = config;
    }

    public Task<string?> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        LastError = null;
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            LastError = "NL2GEO_API_KEY is not set for provider 'openai'.";
            return Task.FromResult<string?>(null);
        }

        // Integration point: wire OpenAI SDK call here.
        LastError = "OpenAI client integration is not implemented in this build.";
        return Task.FromResult<string?>(null);
    }
}
