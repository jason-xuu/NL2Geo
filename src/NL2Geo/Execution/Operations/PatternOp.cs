using System;
using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public sealed class PatternOp : IGeometryOperation
{
    public string Type => "pattern";

    public void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter)
    {
        switch (operation.Type.ToLowerInvariant())
        {
            case "array_linear":
            {
                var count = (int)Math.Round(ReadDouble(operation, "count", 3));
                var spacing = ReadDouble(operation, "spacing", 5);
                var axis = ReadString(operation, "axis", "x");
                commandAdapter.ArrayLinearActive(count, spacing, axis);
                commandAdapter.Redraw();
                commandAdapter.WriteLine($"Created linear array: count={count}, spacing={spacing}, axis={axis}.");
                break;
            }
            case "array_grid":
            {
                var countX = (int)Math.Round(ReadDouble(operation, "count_x", 3));
                var countY = (int)Math.Round(ReadDouble(operation, "count_y", 3));
                var spacing = ReadDouble(operation, "spacing", 5);
                commandAdapter.ArrayGridActive(countX, countY, spacing);
                commandAdapter.Redraw();
                commandAdapter.WriteLine($"Created grid array: {countX}x{countY}, spacing={spacing}.");
                break;
            }
            case "array_polar":
            {
                var count = (int)Math.Round(ReadDouble(operation, "count", 6));
                var radius = ReadDouble(operation, "radius", 5);
                commandAdapter.ArrayPolarActive(count, radius);
                commandAdapter.Redraw();
                commandAdapter.WriteLine($"Created polar array: count={count}, radius={radius}.");
                break;
            }
            default:
                commandAdapter.WriteLine($"Unsupported pattern operation '{operation.Type}'.");
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
