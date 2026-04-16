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
}
