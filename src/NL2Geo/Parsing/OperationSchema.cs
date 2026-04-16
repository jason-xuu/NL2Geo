namespace NL2Geo.Parsing;

public record GeometryOperation(string Type, Dictionary<string, object> Params);

public record ParseResult(
    bool Success,
    List<GeometryOperation> Operations,
    List<string> Warnings,
    string? Error = null
);
