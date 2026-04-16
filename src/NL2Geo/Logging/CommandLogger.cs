using System.Text.Json;

namespace NL2Geo.Logging;

public sealed class CommandLogger
{
    private readonly List<string> _entries = new();

    public IReadOnlyList<string> Entries => _entries;

    public void Log(string eventName, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        _entries.Add($"[{DateTimeOffset.UtcNow:O}] {eventName}: {json}");
    }
}
