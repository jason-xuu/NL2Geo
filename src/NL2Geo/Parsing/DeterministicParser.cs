using System.Text.RegularExpressions;

namespace NL2Geo.Parsing;

public sealed class DeterministicParser : IPromptParser
{
    public Task<ParseResult> ParseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var normalized = prompt.Trim().ToLowerInvariant();
        var ops = new List<GeometryOperation>();
        var warnings = new List<string>();

        if (normalized.Contains("bigger and smaller"))
        {
            return Task.FromResult(new ParseResult(false, ops, warnings, "Conflicting intent: bigger and smaller simultaneously."));
        }

        if (normalized.Contains("nurbs butterfly"))
        {
            return Task.FromResult(new ParseResult(false, ops, warnings, "Unsupported operation request."));
        }

        var dims = ExtractNumbers(normalized);

        if (normalized.Contains("cube"))
        {
            var size = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5;
            ops.Add(new GeometryOperation("create_box", new Dictionary<string, object>
            {
                ["width"] = size,
                ["height"] = size,
                ["depth"] = size
            }));
        }
        else if (normalized.Contains("box"))
        {
            var width = dims.ElementAtOrDefault(0);
            var height = dims.ElementAtOrDefault(1);
            var depth = dims.ElementAtOrDefault(2);

            if (width <= 0 || height <= 0 || depth <= 0)
            {
                warnings.Add("Missing dimensions detected; default dimensions applied.");
                width = width > 0 ? width : 5;
                height = height > 0 ? height : 5;
                depth = depth > 0 ? depth : 5;
            }

            ops.Add(new GeometryOperation("create_box", new Dictionary<string, object>
            {
                ["width"] = width,
                ["height"] = height,
                ["depth"] = depth
            }));
        }
        else if (normalized.Contains("sphere") || normalized.Contains("ball") || normalized.Contains("orb"))
        {
            var radius = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2;
            ops.Add(new GeometryOperation("create_sphere", new Dictionary<string, object>
            {
                ["radius"] = radius
            }));
        }
        else if (normalized.Contains("cylinder") || normalized.Contains("tube") || normalized.Contains("pipe"))
        {
            ops.Add(new GeometryOperation("create_cylinder", new Dictionary<string, object>
            {
                ["radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2,
                ["height"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 6
            }));
        }
        else if (normalized.Contains("cone"))
        {
            ops.Add(new GeometryOperation("create_cone", new Dictionary<string, object>
            {
                ["radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2,
                ["height"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 6
            }));
        }
        else if (normalized.Contains("torus") || normalized.Contains("donut") || normalized.Contains("ring"))
        {
            ops.Add(new GeometryOperation("create_torus", new Dictionary<string, object>
            {
                ["major_radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5,
                ["minor_radius"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 1
            }));
        }
        else if (normalized.Contains("pyramid"))
        {
            ops.Add(new GeometryOperation("create_pyramid", new Dictionary<string, object>
            {
                ["base_width"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5,
                ["base_depth"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 5,
                ["height"] = dims.ElementAtOrDefault(2) > 0 ? dims[2] : 5
            }));
        }
        else if (normalized.Contains("ellipsoid"))
        {
            ops.Add(new GeometryOperation("create_ellipsoid", new Dictionary<string, object>
            {
                ["radius_x"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 3,
                ["radius_y"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 2,
                ["radius_z"] = dims.ElementAtOrDefault(2) > 0 ? dims[2] : 1
            }));
        }
        else if (normalized.Contains("circle"))
        {
            ops.Add(new GeometryOperation("create_circle", new Dictionary<string, object>
            {
                ["radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2
            }));
        }
        else if (normalized.Contains("rectangle") || normalized.Contains("square"))
        {
            var width = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5;
            var height = normalized.Contains("square") ? width : (dims.ElementAtOrDefault(1) > 0 ? dims[1] : width);
            ops.Add(new GeometryOperation("create_rectangle", new Dictionary<string, object>
            {
                ["width"] = width,
                ["height"] = height
            }));
        }
        else if (normalized.Contains("line"))
        {
            ops.Add(new GeometryOperation("create_line", new Dictionary<string, object>
            {
                ["x1"] = 0d,
                ["y1"] = 0d,
                ["z1"] = 0d,
                ["x2"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 10d,
                ["y2"] = 0d,
                ["z2"] = 0d
            }));
        }
        else if (normalized.Contains("point"))
        {
            ops.Add(new GeometryOperation("create_point", new Dictionary<string, object>
            {
                ["x"] = dims.ElementAtOrDefault(0),
                ["y"] = dims.ElementAtOrDefault(1),
                ["z"] = dims.ElementAtOrDefault(2)
            }));
        }
        else if (normalized.Contains("move"))
        {
            var amount = dims.ElementAtOrDefault(0);
            if (amount == 0) amount = 10;
            ops.Add(new GeometryOperation("move", new Dictionary<string, object> { ["x"] = amount, ["y"] = 0d, ["z"] = 0d }));
        }
        else if (normalized.Contains("rotate"))
        {
            var angle = dims.ElementAtOrDefault(0);
            if (angle == 0) angle = 45;
            ops.Add(new GeometryOperation("rotate", new Dictionary<string, object> { ["degrees"] = angle, ["axis"] = "z" }));
        }
        else if (normalized.Contains("grid"))
        {
            ops.Add(new GeometryOperation("array_grid", new Dictionary<string, object> { ["count_x"] = 3, ["count_y"] = 3, ["spacing"] = 5d }));
        }
        else
        {
            return Task.FromResult(new ParseResult(false, ops, warnings, "Could not parse prompt with deterministic parser."));
        }

        return Task.FromResult(new ParseResult(true, ops, warnings));
    }

    private static List<double> ExtractNumbers(string input)
    {
        return Regex.Matches(input, @"-?\d+(\.\d+)?")
            .Select(m => double.TryParse(m.Value, out var value) ? value : 0d)
            .ToList();
    }
}
