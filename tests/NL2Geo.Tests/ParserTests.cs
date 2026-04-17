using System.Linq;
using NL2Geo.Parsing;
using Xunit;

namespace NL2Geo.Tests;

public sealed class ParserTests
{
    [Fact]
    public async Task DeterministicParser_ParsesBoxPrompt()
    {
        var parser = new DeterministicParser();
        var result = await parser.ParseAsync("Create a box with width 5 height 3 depth 4");

        Assert.True(result.Success);
        Assert.Single(result.Operations);
        Assert.Equal("create_box", result.Operations[0].Type);
    }

    [Fact]
    public async Task DeterministicParser_RejectsConflictingPrompt()
    {
        var parser = new DeterministicParser();
        var result = await parser.ParseAsync("Make it bigger and smaller at the same time");

        Assert.False(result.Success);
        Assert.Contains("Conflicting intent", result.Error);
    }

    [Fact]
    public async Task DeterministicParser_ParsesMoveUpPrompt()
    {
        var parser = new DeterministicParser();
        var result = await parser.ParseAsync("Move selection up 5");

        Assert.True(result.Success);
        Assert.Single(result.Operations);
        Assert.Equal("move", result.Operations[0].Type);
        Assert.Equal(5d, (double)result.Operations[0].Params["z"]);
    }

    [Fact]
    public async Task DeterministicParser_ParsesGridPromptWithCounts()
    {
        var parser = new DeterministicParser();
        var result = await parser.ParseAsync("Create a 4x4 grid");

        Assert.True(result.Success);
        Assert.Equal(2, result.Operations.Count);
        Assert.Equal("create_box", result.Operations[0].Type);
        Assert.Equal("array_grid", result.Operations[1].Type);
        Assert.Equal(4, Convert.ToInt32(result.Operations[1].Params["count_x"]));
        Assert.Equal(4, Convert.ToInt32(result.Operations[1].Params["count_y"]));
    }

    [Fact]
    public async Task DeterministicParser_ParsesMultiStepPrompt()
    {
        var parser = new DeterministicParser();
        var result = await parser.ParseAsync("Create a sphere radius 3, then move it 5 units up, and rotate 90 degrees");

        Assert.True(result.Success);
        Assert.Equal(3, result.Operations.Count);
        Assert.Equal("create_sphere", result.Operations[0].Type);
        Assert.Equal("move", result.Operations[1].Type);
        Assert.Equal("rotate", result.Operations[2].Type);
    }

    [Fact]
    public async Task DeterministicParser_ParsesTransformAndGridExamples()
    {
        var parser = new DeterministicParser();

        var moveX = await parser.ParseAsync("Move the selected objects 10 units in X");
        Assert.True(moveX.Success);
        Assert.Equal("move", moveX.Operations.Single().Type);
        Assert.Equal(10d, (double)moveX.Operations.Single().Params["x"]);

        var rotate = await parser.ParseAsync("Rotate selected objects 45 degrees around Z");
        Assert.True(rotate.Success);
        Assert.Equal("rotate", rotate.Operations.Single().Type);
        Assert.Equal("z", rotate.Operations.Single().Params["axis"]);

        var grid = await parser.ParseAsync("Create a 3 by 3 grid of boxes");
        Assert.True(grid.Success);
        Assert.Equal(2, grid.Operations.Count);
        Assert.Equal("create_box", grid.Operations[0].Type);
        Assert.Equal("array_grid", grid.Operations[1].Type);
    }

    [Fact]
    public async Task DeterministicParser_ParsesMultiStepExamplesFromInterviewList()
    {
        var parser = new DeterministicParser();

        var a = await parser.ParseAsync("Create a box 4 4 4 and move it 10 units in X");
        Assert.True(a.Success);
        Assert.Equal(new[] { "create_box", "move" }, a.Operations.Select(o => o.Type).ToArray());

        var b = await parser.ParseAsync("Create a cylinder radius 2 height 6, then rotate it 30 degrees");
        Assert.True(b.Success);
        Assert.Equal(new[] { "create_cylinder", "rotate" }, b.Operations.Select(o => o.Type).ToArray());

        var c = await parser.ParseAsync("Create a 3x3 grid of boxes and rotate them 45 degrees");
        Assert.True(c.Success);
        Assert.Equal(new[] { "create_box", "array_grid", "rotate" }, c.Operations.Select(o => o.Type).ToArray());

        var d = await parser.ParseAsync("Create a sphere radius 3, then move it 5 units up, and rotate 90 degrees");
        Assert.True(d.Success);
        Assert.Equal(new[] { "create_sphere", "move", "rotate" }, d.Operations.Select(o => o.Type).ToArray());
    }
}
