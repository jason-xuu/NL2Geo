using NL2Geo.Adapters;
using NL2Geo.Parsing;
using NL2Geo.Validation;

namespace NL2Geo.Execution;

public interface IGeometryExecutor
{
    void Execute(IReadOnlyList<GeometryOperation> operations, ValidationResult validationResult, IRhinoCommandAdapter adapter);
}
