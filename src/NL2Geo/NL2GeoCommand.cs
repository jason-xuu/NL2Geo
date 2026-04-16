using NL2Geo.Adapters;
using NL2Geo.Execution;
using NL2Geo.Llm;
using NL2Geo.Logging;
using NL2Geo.Parsing;
using NL2Geo.Validation;
using Rhino;
using Rhino.Commands;
using System.Runtime.InteropServices;

namespace NL2Geo;

[Guid("31AE9196-A183-4D90-8AEC-22B0352690CF")]
public sealed class NL2GeoCommand : Command
{
    public override string EnglishName => "NL2GEO";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        string prompt;
        try
        {
            var entered = Rhino.UI.Dialogs.ShowEditBox(
                "NL2GEO",
                "Describe geometry (e.g. 'create a box 5 5 5')",
                string.Empty,
                false,
                out var text);
            if (!entered)
            {
                return Result.Cancel;
            }
            prompt = text ?? string.Empty;
        }
        catch (System.Exception ex)
        {
            RhinoApp.WriteLine($"NL2GEO input error: {ex.Message}");
            return Result.Failure;
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            RhinoApp.WriteLine("Prompt was empty.");
            return Result.Cancel;
        }

        var adapter = new RhinoRuntimeCommandAdapter(doc);
        var engine = CreateEngine(doc);

        EngineResult outcome;
        try
        {
            outcome = engine.ExecutePrompt(prompt, adapter);
        }
        catch (System.Exception ex)
        {
            RhinoApp.WriteLine($"NL2GEO execution error: {ex.Message}");
            return Result.Failure;
        }

        doc?.Views.Redraw();

        return outcome switch
        {
            EngineResult.Success => Result.Success,
            EngineResult.Cancel => Result.Cancel,
            _ => Result.Failure
        };
    }

    internal static NL2GeoEngine CreateEngine(RhinoDoc? doc = null)
    {
        var settings = NL2GeoPlugin.Instance?.PluginConfig ?? new Config.PluginSettings();
        var llmConfig = LlmConfig.FromSettings(settings);

        ILlmClient client = settings.LlmProvider.ToLowerInvariant() switch
        {
            "anthropic" => new AnthropicClient(llmConfig),
            "ollama" => new OllamaClient(llmConfig),
            _ => new OpenAiClient(llmConfig)
        };

        var undo = doc is null
            ? new UndoManager()
            : new UndoManager(
                begin: label => doc.BeginUndoRecord(label),
                end: recordId => doc.EndUndoRecord(recordId));

        return new NL2GeoEngine(
            new LlmPromptParser(client),
            new DeterministicParser(),
            new CommandValidator(),
            new GeometryExecutor(undo),
            new CommandLogger(),
            new PerformanceTracker());
    }
}
