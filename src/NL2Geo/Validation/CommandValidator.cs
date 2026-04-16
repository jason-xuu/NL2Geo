using NL2Geo.Adapters;
using NL2Geo.Parsing;

namespace NL2Geo.Validation;

public sealed class CommandValidator : ICommandValidator
{
    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "create_box", "create_cylinder", "create_sphere", "create_cone", "create_torus",
        "create_pyramid", "create_ellipsoid", "create_circle", "create_rectangle",
        "create_line", "create_point", "create_plane", "create_polyline_surface",
        "move", "rotate", "scale",
        "array_linear", "array_grid", "array_polar"
    };

    public ValidationResult Validate(IReadOnlyList<GeometryOperation> operations, IRhinoCommandAdapter commandAdapter)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (operations.Count == 0)
        {
            errors.Add("No operations parsed from prompt.");
            return new ValidationResult(false, errors, warnings);
        }

        foreach (var operation in operations)
        {
            if (!SupportedTypes.Contains(operation.Type))
            {
                errors.Add(
                    $"Unsupported operation '{operation.Type}'. Supported operations: {string.Join(", ", SupportedTypes.Order())}"
                );
                continue;
            }

            if (operation.Type.Equals("move", StringComparison.OrdinalIgnoreCase) ||
                operation.Type.Equals("rotate", StringComparison.OrdinalIgnoreCase) ||
                operation.Type.Equals("scale", StringComparison.OrdinalIgnoreCase))
            {
                if (!commandAdapter.HasSelection())
                {
                    errors.Add($"Operation '{operation.Type}' requires selected objects, but selection is empty.");
                }
            }

            ValidateNumericRanges(operation, errors, warnings);
            ValidateConflicts(operation, errors);
        }

        return new ValidationResult(errors.Count == 0, errors, warnings);
    }

    private static void ValidateNumericRanges(GeometryOperation operation, List<string> errors, List<string> warnings)
    {
        foreach (var kvp in operation.Params)
        {
            if (!TryGetDouble(kvp.Value, out var value))
            {
                continue;
            }

            if (value <= 0 && IsPositiveDimensionKey(kvp.Key))
            {
                errors.Add($"'{kvp.Key}' must be positive for '{operation.Type}'.");
            }

            if (value is > 100000 or < -100000)
            {
                warnings.Add($"'{kvp.Key}' value {value} looks unrealistic.");
            }

            if (value > 0 && value < 0.01 && IsPositiveDimensionKey(kvp.Key))
            {
                warnings.Add($"'{kvp.Key}' value {value} may indicate unit mismatch.");
            }
        }
    }

    private static void ValidateConflicts(GeometryOperation operation, List<string> errors)
    {
        if (operation.Type.Equals("scale", StringComparison.OrdinalIgnoreCase) &&
            operation.Params.TryGetValue("factor", out var factorObj) &&
            TryGetDouble(factorObj, out var factor))
        {
            if (factor == 1.0)
            {
                errors.Add("Scale factor of 1.0 produces no change; clarify intent.");
            }
        }
    }

    private static bool IsPositiveDimensionKey(string key) => key switch
    {
        "width" or "height" or "depth" or "radius" => true,
        "major_radius" or "minor_radius" => true,
        "base_width" or "base_depth" => true,
        "radius_x" or "radius_y" or "radius_z" => true,
        _ => false
    };

    private static bool TryGetDouble(object value, out double parsed)
    {
        parsed = 0;
        return value switch
        {
            double d => (parsed = d) == d,
            float f => (parsed = f) == f,
            int i => (parsed = i) == i,
            long l => (parsed = l) == l,
            string s when double.TryParse(s, out var p) => (parsed = p) == p,
            _ => false
        };
    }
}
