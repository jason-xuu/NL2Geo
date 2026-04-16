using NL2Geo.Adapters;
using NL2Geo.Parsing;
using NL2Geo.Validation;
using Xunit;

namespace NL2Geo.Tests;

public sealed class ValidatorTests
{
    [Fact]
    public void Validator_FailsOnUnsupportedOperation()
    {
        var validator = new CommandValidator();
        var adapter = new RhinoCommandAdapter();
        var ops = new List<GeometryOperation>
        {
            new("create_nurbs_butterfly", new Dictionary<string, object>())
        };

        var result = validator.Validate(ops, adapter);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Unsupported operation"));
    }

    [Fact]
    public void Validator_FailsOnTransformWithoutSelection()
    {
        var validator = new CommandValidator();
        var adapter = new RhinoCommandAdapter { SelectionPresent = false };
        var ops = new List<GeometryOperation>
        {
            new("move", new Dictionary<string, object> { ["x"] = 10d })
        };

        var result = validator.Validate(ops, adapter);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("requires selected objects"));
    }
}
