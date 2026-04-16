namespace NL2Geo.Parsing;

public interface IPromptParser
{
    Task<ParseResult> ParseAsync(string prompt, CancellationToken cancellationToken = default);
}
