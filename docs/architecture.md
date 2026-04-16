# NL2Geo Architecture

## High-level flow

`Prompt → Parsing → Validation → Execution → Logging`

1. `NL2GeoCommand` receives user text.
2. `LlmPromptParser` attempts structured parse.
3. `DeterministicParser` provides fallback parse if needed.
4. `CommandValidator` enforces operation safety and parameter correctness.
5. `GeometryExecutor` dispatches validated operations in one undo scope.
6. `CommandLogger` + `PerformanceTracker` persist observability data.

## Module responsibilities

- `Parsing/`
  - `IPromptParser` contract.
  - `LlmPromptParser` for natural-language interpretation.
  - `DeterministicParser` for resilient fallback and known command forms.
  - `OperationSchema` for typed operation records.
- `Validation/`
  - `CommandValidator` enforces constraints, edge-case handling, and friendly diagnostics.
- `Execution/`
  - `GeometryExecutor` orchestrates operation dispatch.
  - `Operations/*` implement concrete behavior by operation category.
  - `UndoManager` groups command executions for one-step undo.
- `Adapters/`
  - Rhino interaction boundaries for command input and preview surfaces.
- `Llm/`
  - Provider clients, config, and prompt templates.
- `Logging/`
  - Structured command logs and phase timing.

## Design guarantees

- Interface-first architecture for swappability and testability.
- Deterministic fallback when LLM parsing fails.
- Validation before execution to prevent unsafe geometry operations.
- Centralized edge-case handling:
  - unreasonable units/ranges,
  - missing dimensions,
  - conflicting intent,
  - unsupported operations,
  - null selection for transform commands.
