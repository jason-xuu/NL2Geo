# NL2Geo Rhino Plugin

`NL2Geo` is a Rhino-oriented plugin architecture that converts natural language prompts into validated geometry operations with deterministic fallback paths, structured logging, and measurable performance.

## What this project demonstrates

- C#/.NET plugin architecture with interface-first boundaries.
- Prompt parsing via LLM client and deterministic parser fallback.
- Command validation for safety, parameter bounds, and edge-case handling.
- Execution pipeline with undo-safe grouping and operation dispatch.
- Testable modular design (`IPromptParser`, `ICommandValidator`, `IGeometryExecutor`, `ILlmClient`).

## Project layout

See `docs/architecture.md` for module responsibilities and flow.

## Build and test

Prerequisites:

- .NET 7 SDK
- (Optional for full runtime integration) Rhino 8 + RhinoCommon runtime

Commands:

```bash
dotnet restore NL2Geo.sln
dotnet build NL2Geo.sln -c Release
dotnet test NL2Geo.sln -c Release
```

After build, Rhino-loadable artifacts are emitted automatically:

- `src/NL2Geo/bin/Release/net7.0/NL2Geo.rhp`
- `src/NL2Geo/bin/Release/net8.0/NL2Geo.rhp`

Use the `.rhp` artifact for Rhino plugin loading on macOS.

## Usage model

`NL2GEO` command flow:

1. Parse prompt using `LlmPromptParser`.
2. Fall back to `DeterministicParser` when LLM is unavailable/unusable.
3. Validate all operations and parameters.
4. Execute inside one undo record scope.
5. Log prompt, parsed output, validation, execution timings.

### Local Ollama configuration (optional)

You can run prompt parsing fully local (no paid API) by setting environment variables before launching Rhino:

```bash
export NL2GEO_LLM_PROVIDER=ollama
export NL2GEO_LLM_MODEL=llama3.1:8b
export NL2GEO_OLLAMA_BASE_URL=http://localhost:11434
```

Or use the helper script:

```bash
chmod +x run_rhino_with_ollama.sh
./run_rhino_with_ollama.sh
```

For cloud providers, use:

```bash
export NL2GEO_LLM_PROVIDER=openai   # or anthropic
export NL2GEO_API_KEY=your_key_here
```

If Ollama is unreachable, the command output now reports actionable errors (timeout, bad URL, HTTP status, invalid JSON) and then falls back to deterministic parsing.

## Known limitations

- Integration testing against real Rhino geometry requires Rhino 8 at runtime.
- This repository includes Rhino-facing abstractions and command flow; full `.rhp` packaging is environment-specific.
- LLM clients are wired for API integration but intentionally conservative in local/offline mode.
