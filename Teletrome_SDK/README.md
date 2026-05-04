# Teletrome SDK

Small JS module that gets bundled into an instrumented Chrome extension. The Instrumenter injects calls to this SDK at every named function entry; the SDK is responsible for buffering those calls and shipping them to the Ingest API.

## Role in the pipeline

```
extension source ──► Instrumenter ──► /instrumented copy ──► loaded in Chrome ──► SDK (this) ──► Ingest API
```

This is the **runtime** half of the system. It runs inside the user's browser, inside the extension's MV3 service worker and any other contexts the extension uses.

## Public surface

A single function:

```js
track(functionName, fileName)
```

Everything else is internal. The Instrumenter is the only intended caller; humans should not write `track(...)` by hand.

## Responsibilities

- Buffer events in `chrome.storage.local` so nothing is lost when the MV3 service worker goes dormant between calls.
- Flush the buffer to the Ingest API on an interval (target: every 30 seconds) **and** on `chrome.runtime.onSuspend`.
- Generate a stable **install ID** on first run, persist it in `chrome.storage.local`, and attach it to every payload. This is what lets the report count "unique installs that triggered this function."
- Retry failed flushes with simple exponential backoff. Drop nothing on transient network failure; drop nothing on a stuck request either — bound the retry queue.
- Read the API endpoint and project API key from build-time config (baked in by the Instrumenter).

## MV3 considerations

The service worker can be killed at any time. The SDK must:

- Treat in-memory state as throwaway. The buffer of record is `chrome.storage.local`.
- Register the flush hook on `chrome.runtime.onSuspend` so a final flush runs before the worker dies.
- Avoid long-lived timers as the only flush trigger — they don't survive worker restarts.

## Payload shape (sent to `POST /api/events`)

A batch of events, each carrying:

- `functionName`
- `fileName`
- `installId`
- `recordedAt` (client-side timestamp)

The project is identified by the API key in the request header, not in the payload.

## Performance contract

`track()` is on the hot path of **every named function** in the host extension. It must:

- Return synchronously and do no work beyond appending to an in-memory queue (with a periodic drain to `chrome.storage.local`).
- Never throw into the host extension's call stack.
- Add no measurable overhead vs. the un-instrumented original.

## Out of scope

- Privacy / consent UI. Beta users are developers who know what's running.
- Branch or line coverage. Function-entry events only.
- Sampling. Every call is recorded for the MVP; aggregation happens server-side.
