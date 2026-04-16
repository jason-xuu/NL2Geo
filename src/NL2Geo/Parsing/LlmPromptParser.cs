using System.Text.Json;
using NL2Geo.Llm;

namespace NL2Geo.Parsing;

public sealed class LlmPromptParser : IPromptParser
{
    private readonly ILlmClient _client;

    public LlmPromptParser(ILlmClient client)
    {
        _client = client;
    }

    public async Task<ParseResult> ParseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        string? payload;
        try
        {
            payload = await _client.CompleteAsync(PromptTemplates.SystemPromptV3, prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            return new ParseResult(
                false,
                new List<GeometryOperation>(),
                new List<string>(),
                $"LLM provider '{_client.Name}' threw an exception: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            var details = string.IsNullOrWhiteSpace(_client.LastError)
                ? "LLM unavailable or returned empty response."
                : _client.LastError!;
            return new ParseResult(false, new List<GeometryOperation>(), new List<string>(), details);
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var operations = new List<GeometryOperation>();
            var warnings = new List<string>();

            if (root.TryGetProperty("warnings", out var warningsElement) && warningsElement.ValueKind == JsonValueKind.Array)
            {
                warnings.AddRange(
                    warningsElement.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                );
            }

            if (root.TryGetProperty("operations", out var opsElement) && opsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var op in opsElement.EnumerateArray())
                {
                    if (!op.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var type = typeElement.GetString()!;
                    var parameters = new Dictionary<string, object>();
                    if (op.TryGetProperty("params", out var paramsElement) && paramsElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in paramsElement.EnumerateObject())
                        {
                            parameters[property.Name] = property.Value.ValueKind switch
                            {
                                JsonValueKind.Number when property.Value.TryGetDouble(out var d) => d,
                                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                _ => property.Value.ToString()
                            };
                        }
                    }

                    operations.Add(new GeometryOperation(type, parameters));
                }
            }

            return operations.Count == 0
                ? new ParseResult(false, operations, warnings, "LLM parse produced zero operations.")
                : new ParseResult(true, operations, warnings);
        }
        catch (JsonException ex)
        {
            return new ParseResult(false, new List<GeometryOperation>(), new List<string>(), $"Invalid JSON from LLM: {ex.Message}");
        }
    }
}
