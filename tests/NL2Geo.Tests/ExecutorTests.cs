using System;
using NL2Geo.Adapters;
using NL2Geo.Execution;
using NL2Geo.Parsing;
using NL2Geo.Validation;
using Xunit;

namespace NL2Geo.Tests;

public sealed class ExecutorTests
{
    [Fact]
    public void Executor_ExecutesValidOperations()
    {
        var executor = new GeometryExecutor(new UndoManager());
        var adapter = new RhinoCommandAdapter { SelectionPresent = true };
        var operations = new List<GeometryOperation>
        {
            new("create_box", new Dictionary<string, object> { ["width"] = 5d, ["height"] = 4d, ["depth"] = 3d }),
            new("move", new Dictionary<string, object> { ["x"] = 10d, ["y"] = 0d, ["z"] = 0d })
        };
        var validation = new ValidationResult(true, new List<string>(), new List<string>());

        executor.Execute(operations, validation, adapter);
        Assert.True(adapter.OutputLog.Count >= 2);
        Assert.Contains(adapter.OutputLog, m => m.StartsWith("move:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Executor_ExecutesPatternOperations()
    {
        var executor = new GeometryExecutor(new UndoManager());
        var adapter = new RhinoCommandAdapter { SelectionPresent = true };
        var operations = new List<GeometryOperation>
        {
            new("array_grid", new Dictionary<string, object> { ["count_x"] = 3d, ["count_y"] = 3d, ["spacing"] = 5d })
        };
        var validation = new ValidationResult(true, new List<string>(), new List<string>());

        executor.Execute(operations, validation, adapter);
        Assert.Contains(adapter.OutputLog, m => m.StartsWith("array_grid:", StringComparison.OrdinalIgnoreCase));
    }
}
