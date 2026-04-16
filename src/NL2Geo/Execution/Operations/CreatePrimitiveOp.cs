using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Execution.Operations;

public sealed class CreatePrimitiveOp : IGeometryOperation
{
    public string Type => "create_primitive";

    public void Execute(GeometryOperation operation, IRhinoCommandAdapter commandAdapter)
    {
        var type = operation.Type.ToLowerInvariant();
        System.Guid? id = null;
        switch (type)
        {
            case "create_box":
            {
                var width = ReadDouble(operation, "width", 5);
                var height = ReadDouble(operation, "height", 5);
                var depth = ReadDouble(operation, "depth", 5);
                id = commandAdapter.AddBox(width, height, depth);
                commandAdapter.WriteLine($"Created box ({width} x {height} x {depth}) id={id}");
                break;
            }
            case "create_cylinder":
            {
                var radius = ReadDouble(operation, "radius", 2);
                var height = ReadDouble(operation, "height", 6);
                id = commandAdapter.AddCylinder(radius, height);
                commandAdapter.WriteLine($"Created cylinder (r={radius}, h={height}) id={id}");
                break;
            }
            case "create_sphere":
            {
                var radius = ReadDouble(operation, "radius", 2);
                id = commandAdapter.AddSphere(radius);
                commandAdapter.WriteLine($"Created sphere (r={radius}) id={id}");
                break;
            }
            case "create_cone":
            {
                var radius = ReadDouble(operation, "radius", 2);
                var height = ReadDouble(operation, "height", 6);
                id = commandAdapter.AddCone(radius, height);
                commandAdapter.WriteLine($"Created cone (r={radius}, h={height}) id={id}");
                break;
            }
            case "create_torus":
            {
                var majorRadius = ReadDouble(operation, "major_radius", 5);
                var minorRadius = ReadDouble(operation, "minor_radius", 1);
                id = commandAdapter.AddTorus(majorRadius, minorRadius);
                commandAdapter.WriteLine($"Created torus (R={majorRadius}, r={minorRadius}) id={id}");
                break;
            }
            case "create_pyramid":
            {
                var baseWidth = ReadDouble(operation, "base_width", 5);
                var baseDepth = ReadDouble(operation, "base_depth", 5);
                var height = ReadDouble(operation, "height", 5);
                id = commandAdapter.AddPyramid(baseWidth, baseDepth, height);
                commandAdapter.WriteLine($"Created pyramid ({baseWidth} x {baseDepth} x {height}) id={id}");
                break;
            }
            case "create_ellipsoid":
            {
                var rx = ReadDouble(operation, "radius_x", 3);
                var ry = ReadDouble(operation, "radius_y", 2);
                var rz = ReadDouble(operation, "radius_z", 1);
                id = commandAdapter.AddEllipsoid(rx, ry, rz);
                commandAdapter.WriteLine($"Created ellipsoid ({rx}, {ry}, {rz}) id={id}");
                break;
            }
            case "create_circle":
            {
                var radius = ReadDouble(operation, "radius", 2);
                id = commandAdapter.AddCircle(radius);
                commandAdapter.WriteLine($"Created circle (r={radius}) id={id}");
                break;
            }
            case "create_rectangle":
            {
                var width = ReadDouble(operation, "width", 5);
                var height = ReadDouble(operation, "height", 5);
                id = commandAdapter.AddRectangle(width, height);
                commandAdapter.WriteLine($"Created rectangle ({width} x {height}) id={id}");
                break;
            }
            case "create_line":
            {
                var x1 = ReadDouble(operation, "x1", 0);
                var y1 = ReadDouble(operation, "y1", 0);
                var z1 = ReadDouble(operation, "z1", 0);
                var x2 = ReadDouble(operation, "x2", 1);
                var y2 = ReadDouble(operation, "y2", 0);
                var z2 = ReadDouble(operation, "z2", 0);
                id = commandAdapter.AddLine(x1, y1, z1, x2, y2, z2);
                commandAdapter.WriteLine($"Created line ({x1},{y1},{z1})->({x2},{y2},{z2}) id={id}");
                break;
            }
            case "create_point":
            {
                var x = ReadDouble(operation, "x", 0);
                var y = ReadDouble(operation, "y", 0);
                var z = ReadDouble(operation, "z", 0);
                id = commandAdapter.AddPoint(x, y, z);
                commandAdapter.WriteLine($"Created point ({x},{y},{z}) id={id}");
                break;
            }
            default:
                commandAdapter.WriteLine($"Unsupported primitive type: {operation.Type}");
                break;
        }

        if (id.HasValue)
        {
            commandAdapter.Redraw();
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
            string s when double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => fallback
        };
    }
}
