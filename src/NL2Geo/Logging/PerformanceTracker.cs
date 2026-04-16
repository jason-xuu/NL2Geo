namespace NL2Geo.Logging;

public sealed class PerformanceTracker
{
    private readonly Dictionary<string, DateTimeOffset> _starts = new();
    private readonly Dictionary<string, long> _durationsMs = new();

    public IReadOnlyDictionary<string, long> DurationsMs => _durationsMs;

    public void Start(string phase)
    {
        _starts[phase] = DateTimeOffset.UtcNow;
    }

    public void Stop(string phase)
    {
        if (!_starts.TryGetValue(phase, out var start))
        {
            return;
        }

        _durationsMs[phase] = (long)(DateTimeOffset.UtcNow - start).TotalMilliseconds;
    }
}
