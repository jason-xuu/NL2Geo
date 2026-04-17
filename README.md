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

**Interview (macOS):** double-click `../interview-demos/NL2Geo - Open in Finder.command` to jump to this repo in Finder (real testing is still inside Rhino). See `interview-demos/README.md`.

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

## Example prompts

Type `NL2GEO` in the Rhino command line, then paste any of these into the prompt dialog. Both short positional ("box 5 5 5") and verbose ("width 5, height 5, depth 5") styles work; the deterministic parser reads the numbers in order if the LLM isn't used.

### 3D primitives

```text
Create a box 5 5 5
Create a box with width 10, height 3, and depth 4 meters
Make a cube 6
Create a sphere radius 4
Draw a ball with radius 2.5
Create a cylinder radius 2 height 8
Make a tube radius 1 height 12
Create a cone radius 3 height 10
Create a torus 5 1
Make a donut major radius 6 minor radius 0.5
Create a pyramid 5 5 8
Create an ellipsoid 3 2 1
```

### 2D geometry

```text
Create a circle radius 4
Create a rectangle 10 5
Create a square 6
Create a line 10
Create a point at 2 3 0
```

### Transforms (require an active selection first)

```text
Move the selected objects 10 units in X
Move selection up 5
Rotate selected objects 45 degrees around Z
Rotate selection 30 degrees
```

### Patterns and arrays

```text
Create a 3 by 3 grid of boxes
Create a 4x4 grid
```

### Multi-step prompts

```text
Create a box 4 4 4 and move it 10 units in X
Create a cylinder radius 2 height 6, then rotate it 30 degrees
Create a 3x3 grid of boxes and rotate them 45 degrees
Create a sphere radius 3, then move it 5 units up, and rotate 90 degrees
```

### Graceful-failure / edge-case prompts

These exercise the validator, deterministic fallback, and error reporting rather than producing geometry.

```text
Create a box 0.001mm wide
Create a sphere
Make it bigger and smaller at the same time
Create a NURBS butterfly
Move the selected objects up 5m
```

Expected behavior:

- Tiny dimensions emit a unit-range warning but still run.
- `Create a sphere` with no number falls through to a safe default (radius 2) and adds a warning.
- Conflicting intent (`bigger and smaller`) is rejected with a clear message — nothing is drawn.
- Unsupported operations (`NURBS butterfly`) are refused with a supported-operation hint.
- Transforms run against the current selection; if nothing is selected, the command reports it rather than silently no-op'ing.

After every run the plugin wraps all operations in a single undo record, so `Cmd+Z` reverts the whole prompt atomically.

## Known limitations

- Integration testing against real Rhino geometry requires Rhino 8 at runtime.
- This repository includes Rhino-facing abstractions and command flow; full `.rhp` packaging is environment-specific.
- LLM clients are wired for API integration but intentionally conservative in local/offline mode.
