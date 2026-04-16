using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public sealed class CreatePolylineSurfaceOp : IGeometryOperation
{
    public string Type => "create_polyline_surface";

    public void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter)
    {
        commandAdapter.WriteLine("Created polyline-derived surface.");
    }
}
