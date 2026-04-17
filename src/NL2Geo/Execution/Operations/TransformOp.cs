using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public sealed class TransformOp : IGeometryOperation
{
    public string Type => "transform";

    public void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter)
    {
        switch (operation.Type.ToLowerInvariant())
        {
            case "move":
            {
                var x = ReadDouble(operation, "x", 0);
                var y = ReadDouble(operation, "y", 0);
                var z = ReadDouble(operation, "z", 0);
                commandAdapter.MoveActive(x, y, z);
                commandAdapter.Redraw();
                commandAdapter.WriteLine($"Moved active objects by ({x}, {y}, {z}).");
                break;
            }
            case "rotate":
            {
                var degrees = ReadDouble(operation, "degrees", 45);
                var axis = ReadString(operation, "axis", "z");
                commandAdapter.RotateActive(degrees, axis);
                commandAdapter.Redraw();
                commandAdapter.WriteLine($"Rotated active objects {degrees}° around {axis.ToUpperInvariant()}.");
                break;
            }
            case "scale":
            {
                var factor = ReadDouble(operation, "factor", 1.2);
                commandAdapter.ScaleActive(factor);
                commandAdapter.Redraw();
                commandAdapter.WriteLine($"Scaled active objects by factor {factor}.");
                break;
            }
            default:
                commandAdapter.WriteLine($"Unsupported transform operation '{operation.Type}'.");
                break;
        }
    }

    private static double ReadDouble(GeometryOperation operation, string key, double fallback)
    {
        if (!operation.Params.TryGetValue(key, out var raw) || raw is null)
        {
            return fallback;
        }

        return raw switch
        {
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            string s when double.TryParse(
                s,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => fallback
        };
    }

    private static string ReadString(GeometryOperation operation, string key, string fallback)
    {
        if (!operation.Params.TryGetValue(key, out var raw) || raw is null)
        {
            return fallback;
        }

        return raw.ToString() ?? fallback;
    }
}
