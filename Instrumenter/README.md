# Instrumenter

Node.js CLI tool that takes a Chrome extension project directory as input and outputs an instrumented copy with telemetry hooks injected into every named function. The instrumented copy is what the developer ships (or sideloads) to collect runtime call data from real users.

## Role in the pipeline

```
extension source ──► Instrumenter (this) ──► /instrumented copy ──► loaded in Chrome ──► SDK ──► Ingest API
```

This component is the **build-time** half of the system. It never runs in the user's browser; it produces the code that does.

## Responsibilities

- Walk an input project directory and produce a parallel `/instrumented` directory, leaving the original untouched.
- Parse each JS file with Babel and traverse the AST.
- Wrap every **named function body** with a call to the SDK's `track(functionName, fileName)` hook.
- Inject the SDK import at the top of each transformed file.
- Read `telemetry.config.json` for the API endpoint, project API key, and exclusion list.
- Skip `node_modules`, already-minified files, and any path matched by the config's exclude list.

## Config — `telemetry.config.json`

Lives at the root of the extension project being instrumented.

```json
{
  "apiEndpoint": "https://...",
  "apiKey": "<project api key>",
  "exclude": ["vendor/**", "**/*.min.js"]
}
```

The API key is baked into the instrumented bundle and travels with every event the SDK sends. Treat it as a project identifier, not a secret.

## Usage (target shape)

```
teletrome-instrument <path-to-extension>
```

Output goes to `<path-to-extension>/instrumented`.

## Transform rules

- Only **named** functions are wrapped — anonymous arrow callbacks are skipped to keep the report readable and avoid noise.
- The wrapper records the function name and the source file name. That pair is the unit of "was this code ever called?"
- The transform must be a no-op for performance: a single function call at the top of each function body, no closure rewriting, no async-boundary changes.

## Out of scope

- Branch / line coverage. This component records function-entry events only.
- Audit-vs-production profile switching. Every build is an audit build for the MVP.
- Source maps for the instrumented output (the developer keeps the originals).

## Success criteria (MVP)

- A real extension can be instrumented in under 2 minutes.
- The instrumented extension shows no measurable performance degradation vs. the original.
