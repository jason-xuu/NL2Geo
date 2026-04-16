namespace NL2Geo.Execution;

public interface IUndoManager
{
    IDisposable BeginGroup(string label);
}

public sealed class UndoManager : IUndoManager
{
    private readonly Func<string, uint>? _begin;
    private readonly Action<uint>? _end;

    public UndoManager(Func<string, uint>? begin = null, Action<uint>? end = null)
    {
        _begin = begin;
        _end = end;
    }

    private sealed class UndoScope : IDisposable
    {
        private readonly uint _recordId;
        private readonly Action<uint>? _end;

        public UndoScope(uint recordId, Action<uint>? end)
        {
            _recordId = recordId;
            _end = end;
        }

        public void Dispose()
        {
            if (_recordId > 0)
            {
                _end?.Invoke(_recordId);
            }
        }
    }

    public IDisposable BeginGroup(string label)
    {
        var recordId = _begin?.Invoke(label) ?? 0u;
        return new UndoScope(recordId, _end);
    }
}
