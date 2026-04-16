using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public sealed class PatternOp : IGeometryOperation
{
    public string Type => "pattern";

    public void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter)
    {
        commandAdapter.WriteLine($"Applied pattern '{operation.Type}'.");
    }
}
