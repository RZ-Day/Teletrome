# Teletrome API

ASP.NET Core Web API that ingests telemetry events from instrumented Chrome extensions and serves the dead-code report. Backed by SQL Server.

## Role in the pipeline

```
extension source ──► Instrumenter ──► /instrumented copy ──► loaded in Chrome ──► SDK ──► Ingest API (this) ──► SQL Server
                                                                                              │
                                                                                              └──► dead code report
```

This component is the system of record. Every call site that ever fired in a real user's browser ends up here.

## Endpoints

### `POST /api/events`

Accepts a batched event payload from the SDK.

- Authenticated by **project API key** in a request header, validated against the `Projects` table.
- Each event identifies a function by `(functionName, fileName)`. On first sight of a pair for a given project, insert a row into `FunctionRegistry`; on subsequent sightings, look it up.
- Each event also carries an `installId`. Upsert into `InstallSessions` to keep `first_seen` / `last_seen` current.
- Insert one row into `Events` per event in the batch.
- Rate-limited per API key to guard against runaway extensions.

### `GET /api/projects/{id}/report`

Returns the aggregated dead-code report for one project. Per function in the registry:

- Total call count
- Unique install count that triggered it
- Last called date
- `neverCalled` flag — true when the function has zero events across all installs

For the MVP, JSON is enough. A minimal read-only web view is acceptable but not required.

## Database schema (SQL Server)

| Table | Columns |
|---|---|
| `Projects` | `id`, `name`, `api_key`, `created_at` |
| `FunctionRegistry` | `id`, `project_id`, `function_name`, `file_name` — populated on first event seen |
| `Events` | `id`, `function_registry_id`, `install_id`, `recorded_at` — high volume, indexed on project + function |
| `InstallSessions` | `id`, `project_id`, `install_id`, `first_seen`, `last_seen` |

`Events` is the hot table. The `(project_id, function_registry_id)` index is what makes the report query tractable; without it the dead-code aggregation walks the full table.

## Auth model

- One API key per project, stored on the `Projects` row.
- The SDK sends the key in a header on every `POST /api/events`.
- Project provisioning is **manual** for the MVP — beta users get a row inserted by hand. No self-serve signup.

## Rate limiting

Per API key, on the ingest endpoint. A misbehaving extension that calls `track()` in a tight loop should not be able to take down the API or run up unbounded storage cost for the project owner.

## Out of scope

- Self-serve project registration.
- Dashboard UI beyond the basic report view.
- Audit vs. production profile switching.
- Branch / line coverage aggregation.

## Success criteria (MVP)

- Events flow end to end and appear in the report within 60 seconds of being triggered.
- The report correctly identifies at least one function that was never called across all beta installs.
