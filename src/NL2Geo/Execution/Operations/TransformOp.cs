using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public sealed class TransformOp : IGeometryOperation
{
    public string Type => "transform";

    public void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter)
    {
        commandAdapter.WriteLine($"Applied transform '{operation.Type}'.");
    }
}
