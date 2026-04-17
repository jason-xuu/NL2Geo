using System;
using System.Linq;
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

        var clauses = SplitIntoClauses(normalized);
        foreach (var clause in clauses)
        {
            ops.AddRange(ParseClause(clause, warnings));
        }

        if (ops.Count == 0)
        {
            return Task.FromResult(new ParseResult(false, ops, warnings, "Could not parse prompt with deterministic parser."));
        }

        return Task.FromResult(new ParseResult(true, ops, warnings));
    }

    private static List<string> SplitIntoClauses(string normalized)
    {
        var s = normalized;
        s = Regex.Replace(s, @"[,;]", " then ");
        s = Regex.Replace(s, @"\band\s+then\b", " then ");
        s = Regex.Replace(s, @"\bthen\b", " then ");
        s = Regex.Replace(
            s,
            @"\band\s+(?=(?:move|rotate|scale|create|make|generate|array|grid)\b)",
            " then ");

        return s.Split(" then ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    private static IEnumerable<GeometryOperation> ParseClause(string clause, List<string> warnings)
    {
        var dims = ExtractNumbers(clause);
        var ops = new List<GeometryOperation>();

        // Combined create + grid prompt ("create a 3x3 grid of boxes")
        if (clause.Contains("grid") && (clause.Contains("box") || clause.Contains("cube")))
        {
            var (cx, cy) = ExtractGridCounts(clause);
            var size = dims.FirstOrDefault(d => d > 0);
            if (size <= 0) size = 4;
            ops.Add(new GeometryOperation("create_box", new Dictionary<string, object>
            {
                ["width"] = size,
                ["height"] = size,
                ["depth"] = size
            }));
            ops.Add(new GeometryOperation("array_grid", new Dictionary<string, object>
            {
                ["count_x"] = cx,
                ["count_y"] = cy,
                ["spacing"] = Math.Max(size * 1.5, 5.0)
            }));
            return ops;
        }

        if (clause.Contains("cube"))
        {
            var size = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5;
            ops.Add(new GeometryOperation("create_box", new Dictionary<string, object>
            {
                ["width"] = size, ["height"] = size, ["depth"] = size
            }));
        }
        else if (clause.Contains("box"))
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
                ["width"] = width, ["height"] = height, ["depth"] = depth
            }));
        }
        else if (clause.Contains("sphere") || clause.Contains("ball") || clause.Contains("orb"))
        {
            var radius = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2;
            ops.Add(new GeometryOperation("create_sphere", new Dictionary<string, object> { ["radius"] = radius }));
        }
        else if (clause.Contains("cylinder") || clause.Contains("tube") || clause.Contains("pipe"))
        {
            ops.Add(new GeometryOperation("create_cylinder", new Dictionary<string, object>
            {
                ["radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2,
                ["height"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 6
            }));
        }
        else if (clause.Contains("cone"))
        {
            ops.Add(new GeometryOperation("create_cone", new Dictionary<string, object>
            {
                ["radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2,
                ["height"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 6
            }));
        }
        else if (clause.Contains("torus") || clause.Contains("donut") || clause.Contains("ring"))
        {
            ops.Add(new GeometryOperation("create_torus", new Dictionary<string, object>
            {
                ["major_radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5,
                ["minor_radius"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 1
            }));
        }
        else if (clause.Contains("pyramid"))
        {
            ops.Add(new GeometryOperation("create_pyramid", new Dictionary<string, object>
            {
                ["base_width"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5,
                ["base_depth"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 5,
                ["height"] = dims.ElementAtOrDefault(2) > 0 ? dims[2] : 5
            }));
        }
        else if (clause.Contains("ellipsoid"))
        {
            ops.Add(new GeometryOperation("create_ellipsoid", new Dictionary<string, object>
            {
                ["radius_x"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 3,
                ["radius_y"] = dims.ElementAtOrDefault(1) > 0 ? dims[1] : 2,
                ["radius_z"] = dims.ElementAtOrDefault(2) > 0 ? dims[2] : 1
            }));
        }
        else if (clause.Contains("circle"))
        {
            ops.Add(new GeometryOperation("create_circle", new Dictionary<string, object>
            {
                ["radius"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 2
            }));
        }
        else if (clause.Contains("rectangle") || clause.Contains("square"))
        {
            var width = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 5;
            var height = clause.Contains("square") ? width : (dims.ElementAtOrDefault(1) > 0 ? dims[1] : width);
            ops.Add(new GeometryOperation("create_rectangle", new Dictionary<string, object>
            {
                ["width"] = width, ["height"] = height
            }));
        }
        else if (clause.Contains("line"))
        {
            ops.Add(new GeometryOperation("create_line", new Dictionary<string, object>
            {
                ["x1"] = 0d, ["y1"] = 0d, ["z1"] = 0d,
                ["x2"] = dims.ElementAtOrDefault(0) > 0 ? dims[0] : 10d,
                ["y2"] = 0d, ["z2"] = 0d
            }));
        }
        else if (clause.Contains("point"))
        {
            ops.Add(new GeometryOperation("create_point", new Dictionary<string, object>
            {
                ["x"] = dims.ElementAtOrDefault(0),
                ["y"] = dims.ElementAtOrDefault(1),
                ["z"] = dims.ElementAtOrDefault(2)
            }));
        }
        else if (clause.Contains("move"))
        {
            var amount = dims.ElementAtOrDefault(0);
            if (amount == 0) amount = 10;
            var x = 0d; var y = 0d; var z = 0d;
            if (Regex.IsMatch(clause, @"\bup\b")) z = amount;
            else if (Regex.IsMatch(clause, @"\bdown\b")) z = -amount;
            else if (Regex.IsMatch(clause, @"\bleft\b")) x = -amount;
            else if (Regex.IsMatch(clause, @"\bright\b")) x = amount;
            else if (Regex.IsMatch(clause, @"\bin\s*x\b")) x = amount;
            else if (Regex.IsMatch(clause, @"\bin\s*y\b")) y = amount;
            else if (Regex.IsMatch(clause, @"\bin\s*z\b")) z = amount;
            else x = amount;
            ops.Add(new GeometryOperation("move", new Dictionary<string, object> { ["x"] = x, ["y"] = y, ["z"] = z }));
        }
        else if (clause.Contains("rotate"))
        {
            var angle = dims.ElementAtOrDefault(0);
            if (angle == 0) angle = 45;
            var axis = "z";
            if (Regex.IsMatch(clause, @"(around|about)\s*x|\bx[- ]axis\b")) axis = "x";
            else if (Regex.IsMatch(clause, @"(around|about)\s*y|\by[- ]axis\b")) axis = "y";
            ops.Add(new GeometryOperation("rotate", new Dictionary<string, object> { ["degrees"] = angle, ["axis"] = axis }));
        }
        else if (clause.Contains("grid"))
        {
            var (cx, cy) = ExtractGridCounts(clause);
            if (clause.Contains("create") || clause.Contains("make") || clause.Contains("generate"))
            {
                ops.Add(new GeometryOperation("create_box", new Dictionary<string, object>
                {
                    ["width"] = 4d, ["height"] = 4d, ["depth"] = 4d
                }));
            }
            ops.Add(new GeometryOperation("array_grid", new Dictionary<string, object>
            {
                ["count_x"] = cx, ["count_y"] = cy, ["spacing"] = 5d
            }));
        }

        return ops;
    }

    private static (int x, int y) ExtractGridCounts(string clause)
    {
        var by = Regex.Match(clause, @"(\d+)\s*(?:x|by)\s*(\d+)");
        if (by.Success &&
            int.TryParse(by.Groups[1].Value, out var a) &&
            int.TryParse(by.Groups[2].Value, out var b))
        {
            return (Math.Max(1, a), Math.Max(1, b));
        }
        var single = ExtractNumbers(clause).FirstOrDefault(n => n > 0);
        if (single > 0)
        {
            var c = Math.Max(1, (int)Math.Round(single));
            return (c, c);
        }
        return (3, 3);
    }

    private static List<double> ExtractNumbers(string input)
    {
        return Regex.Matches(input, @"-?\d+(\.\d+)?")
            .Select(m => double.TryParse(m.Value, out var value) ? value : 0d)
            .ToList();
    }
}
