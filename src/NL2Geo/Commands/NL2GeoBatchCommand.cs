using NL2Geo.Adapters;

namespace NL2Geo.Commands;

public sealed class NL2GeoBatchCommand
{
    private readonly NL2GeoEngine _engine;

    public NL2GeoBatchCommand(NL2GeoEngine engine)
    {
        _engine = engine;
    }

    public IReadOnlyList<EngineResult> RunBatch(IEnumerable<string> prompts, IRhinoCommandAdapter adapter)
    {
        var results = new List<EngineResult>();
        foreach (var prompt in prompts)
        {
            results.Add(_engine.ExecutePrompt(prompt, adapter));
        }

        return results;
    }
}
