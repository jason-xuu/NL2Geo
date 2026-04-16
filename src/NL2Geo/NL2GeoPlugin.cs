using NL2Geo.Config;
using NL2Geo.Llm;
using Rhino.PlugIns;
using System.Runtime.InteropServices;

namespace NL2Geo;

[Guid("0F69A7E8-6B21-44CF-B4B8-19A1C71C1D42")]
public sealed class NL2GeoPlugin : PlugIn
{
    public static NL2GeoPlugin? Instance { get; private set; }

    public PluginSettings PluginConfig { get; }
    public LlmConfig LlmConfig => LlmConfig.FromSettings(PluginConfig);

    public NL2GeoPlugin()
    {
        Instance = this;
        PluginConfig = new PluginSettings();
    }
}
