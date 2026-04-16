using NL2Geo.Adapters;
using NL2Geo.Execution;
using NL2Geo.Logging;
using NL2Geo.Parsing;
using NL2Geo.Validation;
using Xunit;

namespace NL2Geo.Tests;

public sealed class IntegrationTests
{
    [Fact]
    public void Command_EndToEnd_UsesFallbackAndValidates()
    {
        var llmParser = new StubFailingParser();
        var deterministic = new DeterministicParser();
        var validator = new CommandValidator();
        var executor = new GeometryExecutor(new UndoManager());
        var logger = new CommandLogger();
        var perf = new PerformanceTracker();
        var engine = new NL2GeoEngine(llmParser, deterministic, validator, executor, logger, perf);
        var adapter = new RhinoCommandAdapter { SelectionPresent = true };

        var result = engine.ExecutePrompt("Create a box width 5 height 3 depth 4", adapter);

        Assert.Equal(EngineResult.Success, result);
        Assert.NotEmpty(logger.Entries);
        Assert.Contains("Created box", adapter.OutputLog[0]);
        Assert.Contains("box:5x3x4", adapter.AddedGeometry);
    }

    private sealed class StubFailingParser : IPromptParser
    {
        public Task<ParseResult> ParseAsync(string prompt, CancellationToken cancellationToken = default)
            => Task.FromResult(new ParseResult(false, new List<GeometryOperation>(), new List<string>(), "offline"));
    }
}
