namespace NL2Geo.Validation;

public sealed record ValidationResult(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
);
