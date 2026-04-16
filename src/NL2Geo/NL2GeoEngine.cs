using NL2Geo.Adapters;
using NL2Geo.Execution;
using NL2Geo.Logging;
using NL2Geo.Parsing;
using NL2Geo.Validation;

namespace NL2Geo;

public enum EngineResult
{
    Success,
    Failure,
    Cancel
}

public sealed class NL2GeoEngine
{
    private readonly IPromptParser _llmParser;
    private readonly IPromptParser _deterministicParser;
    private readonly ICommandValidator _validator;
    private readonly IGeometryExecutor _executor;
    private readonly CommandLogger _logger;
    private readonly PerformanceTracker _performance;

    public NL2GeoEngine(
        IPromptParser llmParser,
        IPromptParser deterministicParser,
        ICommandValidator validator,
        IGeometryExecutor executor,
        CommandLogger logger,
        PerformanceTracker performance)
    {
        _llmParser = llmParser;
        _deterministicParser = deterministicParser;
        _validator = validator;
        _executor = executor;
        _logger = logger;
        _performance = performance;
    }

    public EngineResult ExecutePrompt(string prompt, IRhinoCommandAdapter adapter)
    {
        _performance.Start("parse");
        // Marshal async parsing onto the thread pool so we never capture the
        // caller's SynchronizationContext (e.g. Rhino's UI thread) when awaiting.
        var parse = Task.Run(() => _llmParser.ParseAsync(prompt)).GetAwaiter().GetResult();
        if (!parse.Success)
        {
            parse = Task.Run(() => _deterministicParser.ParseAsync(prompt)).GetAwaiter().GetResult();
        }
        _performance.Stop("parse");

        _logger.Log("parse", parse);
        if (!parse.Success)
        {
            adapter.WriteLine(parse.Error ?? "Failed to parse prompt.");
            return EngineResult.Failure;
        }

        _performance.Start("validate");
        var validation = _validator.Validate(parse.Operations, adapter);
        _performance.Stop("validate");
        _logger.Log("validate", validation);

        _performance.Start("execute");
        _executor.Execute(parse.Operations, validation, adapter);
        _performance.Stop("execute");
        _logger.Log("performance", _performance.DurationsMs);

        return validation.IsValid ? EngineResult.Success : EngineResult.Failure;
    }
}
