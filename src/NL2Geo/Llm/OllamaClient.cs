using System.Text;
using System.Text.Json;

namespace NL2Geo.Llm;

public sealed class OllamaClient : ILlmClient
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(120)
    };
    private readonly LlmConfig _config;
    public string Name => "ollama";
    public string? LastError { get; private set; }

    public OllamaClient(LlmConfig config)
    {
        _config = config;
    }

    public async Task<string?> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        LastError = null;
        try
        {
            var baseUrl = _config.OllamaBaseUrl.TrimEnd('/');
            var endpoint = $"{baseUrl}/api/chat";

            var payload = new
            {
                model = _config.Model,
                stream = false,
                format = "json",
                options = new
                {
                    temperature = _config.Temperature
                },
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await Http.PostAsync(endpoint, content, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                LastError = $"Ollama HTTP {(int)response.StatusCode}: {responseBody}";
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            var root = document.RootElement;

            if (root.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var messageContent) &&
                messageContent.ValueKind == JsonValueKind.String)
            {
                return messageContent.GetString();
            }

            if (root.TryGetProperty("response", out var responseField) &&
                responseField.ValueKind == JsonValueKind.String)
            {
                return responseField.GetString();
            }

            LastError = "Ollama response missing message content.";
            return null;
        }
        catch (TaskCanceledException)
        {
            LastError = $"Ollama request timed out after {Http.Timeout.TotalSeconds:0}s. Ensure Ollama is running and model '{_config.Model}' is pulled.";
            return null;
        }
        catch (HttpRequestException ex)
        {
            LastError = $"Failed to reach Ollama at '{_config.OllamaBaseUrl}': {ex.Message}";
            return null;
        }
        catch (JsonException ex)
        {
            LastError = $"Invalid JSON returned by Ollama: {ex.Message}";
            return null;
        }
    }
}
