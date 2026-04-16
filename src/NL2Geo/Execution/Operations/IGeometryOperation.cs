using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public interface IGeometryOperation
{
    string Type { get; }
    void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter);
}
