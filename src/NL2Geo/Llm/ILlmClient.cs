namespace NL2Geo.Llm;

public interface ILlmClient
{
    string Name { get; }
    string? LastError { get; }
    Task<string?> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
