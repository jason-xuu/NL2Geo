namespace NL2Geo.Llm;

public sealed class AnthropicClient : ILlmClient
{
    private readonly LlmConfig _config;
    public string Name => "anthropic";
    public string? LastError { get; private set; }

    public AnthropicClient(LlmConfig config)
    {
        _config = config;
    }

    public Task<string?> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        LastError = null;
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            LastError = "NL2GEO_API_KEY is not set for provider 'anthropic'.";
            return Task.FromResult<string?>(null);
        }

        // Integration point: wire Anthropic SDK call here.
        LastError = "Anthropic client integration is not implemented in this build.";
        return Task.FromResult<string?>(null);
    }
}
