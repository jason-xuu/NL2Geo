using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Validation;

public interface ICommandValidator
{
    ValidationResult Validate(IReadOnlyList<GeometryOperation> operations, IRhinoCommandAdapter commandAdapter);
}
