using NL2Geo.Adapters;
using NL2Geo.Execution.Operations;
using NL2Geo.Parsing;
using NL2Geo.Validation;

namespace NL2Geo.Execution;

public sealed class GeometryExecutor : IGeometryExecutor
{
    private readonly IUndoManager _undoManager;
    private readonly IReadOnlyList<IGeometryOperation> _operationHandlers;

    public GeometryExecutor(IUndoManager undoManager)
    {
        _undoManager = undoManager;
        _operationHandlers = new List<IGeometryOperation>
        {
            new CreatePrimitiveOp(),
            new CreatePolylineSurfaceOp(),
            new TransformOp(),
            new PatternOp()
        };
    }

    public void Execute(IReadOnlyList<GeometryOperation> operations, ValidationResult validationResult, IRhinoCommandAdapter adapter)
    {
        if (!validationResult.IsValid)
        {
            adapter.WriteLine("Execution aborted due to validation errors.");
            foreach (var error in validationResult.Errors)
            {
                adapter.WriteLine($"Error: {error}");
            }
            return;
        }

        using var scope = _undoManager.BeginGroup("NL2GEO");
        foreach (var operation in operations)
        {
            var handler = Resolve(operation.Type);
            if (handler is null)
            {
                adapter.WriteLine($"Skipped unsupported execution type: {operation.Type}");
                continue;
            }

            handler.Execute(operation, adapter);
        }
    }

    private IGeometryOperation? Resolve(string operationType)
    {
        if (operationType.StartsWith("create_", StringComparison.OrdinalIgnoreCase))
        {
            return operationType.Equals("create_polyline_surface", StringComparison.OrdinalIgnoreCase)
                ? _operationHandlers.OfType<CreatePolylineSurfaceOp>().FirstOrDefault()
                : _operationHandlers.OfType<CreatePrimitiveOp>().FirstOrDefault();
        }

        if (operationType is "move" or "rotate" or "scale")
        {
            return _operationHandlers.OfType<TransformOp>().FirstOrDefault();
        }

        if (operationType.StartsWith("array_", StringComparison.OrdinalIgnoreCase))
        {
            return _operationHandlers.OfType<PatternOp>().FirstOrDefault();
        }

        return null;
    }
}
