using NL2Geo.Adapters;

namespace NL2Geo.Commands;

public sealed class NL2GeoInteractiveCommand
{
    private readonly NL2GeoEngine _engine;

    public NL2GeoInteractiveCommand(NL2GeoEngine engine)
    {
        _engine = engine;
    }

    public EngineResult RunInteractive(string prompt, IRhinoCommandAdapter adapter)
    {
        adapter.WriteLine("Interactive NL2GEO execution started.");
        return _engine.ExecutePrompt(prompt, adapter);
    }
}
