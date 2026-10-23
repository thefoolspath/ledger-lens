# 0007.02 — Centralize API exception handling and structured logging

Last reviewed: 2026-09-28.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: API contract and failure boundary

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [Plan 0004](../0004-unified-api-response-contract.md) — remains the implementation owner for typed expected failures and Problem Details closeout.
- [0007.01](01-created-location-policy.md) — shares response-contract and Gateway integration coverage.
- [0007.04](04-collection-correctness-pagination-and-sorting.md) — page validation and invalid sort/page input require typed failures.
- [0007.06](06-request-lifecycle-timeouts-mutations-and-exports.md) — cancellation, timeout, provider failure, and unexpected-failure classification must remain distinct.


**Status:** Accepted direction; implementation not started and remains governed by plan 0004.

### Current behavior

All current API hosts register shared Problem Details support and call `UseExceptionHandler()`. `LedgerLens.ServiceDefaults` also configures OpenTelemetry logging, tracing, and metrics; when an OTLP endpoint is supplied, those signals are exported to the configured collector and are visible in the local Aspire workflow.

Portfolio Core endpoints still contain repeated local `try/catch` blocks. They currently translate broad exception types as follows:

- `ArgumentException` becomes `400 Bad Request` with `common.validation.invalid_request`.
- `InvalidOperationException` becomes `409 Conflict` where that catch is present.

`MarketDataEndpointExecution` similarly catches `ArgumentException` as `400` and `MarketDataUnavailableException` as `503 Service Unavailable`.

These caught exceptions do not reach the exception-handling middleware. The local catches return an HTTP result without writing an exception log, so request telemetry may show the final status while the cause and stack are not recorded by the centralized exception pipeline. `ApiProblemFactory` currently logs only missing localization resources; producing an ordinary validation, not-found, conflict, or service-unavailable response does not itself emit an error log.

The current observability setup emits logs but does not introduce a durable repository-owned log store. Aspire Dashboard is the Version 1 observability UI; retention or a production logging backend remains a separate decision.

### Review concern

Exception-to-HTTP translation and exception logging are cross-cutting request-pipeline responsibilities and should not be repeated in individual endpoints. Endpoints should validate transport-specific inputs, call their exact handler, and map typed expected results; unexpected exceptions should flow to one centralized handler.

Moving the existing catches mechanically is unsafe without classification work:

- `ArgumentException` can represent invalid client input, but it can also be raised by invalid fixed-user configuration or another programming/configuration defect. Mapping every `ArgumentException` globally to `400` could conceal a server fault as a client error.
- `InvalidOperationException` is similarly broader than a business-state conflict and must not automatically mean `409` for every service and operation.
- Logging a complete exception object can include exception messages, provider payloads, URLs, personal data, or secrets. Centralized logging must use allowlisted structured fields and preserve the repository's privacy rules.
- Request-aborted `OperationCanceledException` or `TaskCanceledException` must not be reported as an internal system failure when cancellation is expected.
- In .NET 10, exception-handler middleware suppresses its diagnostics by default when a registered `IExceptionHandler` handles the exception. A custom handler must therefore own intentional logging, or middleware diagnostics must be configured explicitly; the design must avoid duplicate records.

### Options

#### Option A — move the current broad mappings into one shared exception handler

- Register a shared `IExceptionHandler` through `LedgerLens.ServiceDefaults` and keep `UseExceptionHandler()` in each API host.
- Log handled `ArgumentException` and `InvalidOperationException` centrally, then map them to the existing `400` and `409` Problem Details responses.
- Add a Market Data-specific handler for `MarketDataUnavailableException` and map it to the existing `503` response.
- Remove the corresponding `try/catch` blocks from Portfolio Core endpoints and `MarketDataEndpointExecution`.

This is the smallest behavior-preserving change, but it retains ambiguous exception-type classification and can misreport server faults as expected client failures.

#### Option B — complete typed expected failures before removing endpoint catches

- Change Application handlers to return typed, transport-independent expected failures for validation, not found, conflict, forbidden, and unavailable cases.
- Map those typed results to Problem Details at the API boundary without throwing exceptions for expected outcomes.
- Add one centralized `IExceptionHandler` for genuinely unexpected exceptions only; log safely and return generic `500` Problem Details.
- Remove local endpoint `try/catch` blocks after each operation has typed failure coverage.

This best matches plan 0004 but has a wider implementation scope because all affected handler contracts and tests change together.

#### Option C — staged centralization with an explicit removal path

- First add centralized exception handling and structured logging, preserving current `400`, `409`, and `503` behavior through a narrowly documented transitional mapping.
- Remove only endpoint catches whose behavior is covered by focused middleware integration tests.
- Record every transitional exception mapping in an allowlist by service and failure purpose rather than treating broad CLR exception types as universally equivalent to HTTP statuses.
- Then migrate expected failures to typed Application results and delete the transitional mappings.
- Keep cancellation handling, privacy-safe logging, and unexpected `500` behavior centralized throughout both stages.

This provides the requested single pipeline sooner while making the broad exception mappings explicitly temporary.

### Accepted decision

Use Option B. Complete typed, transport-independent expected failures operation by operation before removing their local catches. Do not introduce a transitional global mapping from broad CLR exception types to HTTP status codes. `IExceptionHandler` handles genuinely unexpected failures only; expected validation, not-found, conflict, forbidden, and dependency-unavailable outcomes flow through typed Application results and are mapped to Problem Details at the API boundary.

The typed result design must remain small and explicit. Do not replace exception ambiguity with one global result type, god enum, stringly typed error bag, reflection dispatcher, or transport dependency in Application. Share stable failure categories and safe arguments only when multiple operations demonstrate the same contract; keep operation-specific failure data with its owning operation.

### Proposed logging policy for review

- Expected `4xx` outcomes: do not log a warning merely because a safe client error occurred. Emit a structured operational record only when it is actionable, security-relevant, anomalous, or needed for a separately approved metric.
- Dependency/provider `503` outcomes: use the approved operational level based on actionability and expected transience, with a stable error code, allowlisted provider identifier, trace ID, and correlation ID; exclude provider payloads, request URLs containing keys, and credentials.
- Unexpected `500` outcomes: structured `Error` from the centralized handler with exception type, safe diagnostic context, trace ID, and correlation ID; return only generic localized Problem Details to the client.
- Expected request cancellation: do not emit an application error log when `HttpContext.RequestAborted` caused the cancellation.
- Do not add a database table, Seq, Loki, or another durable logging product in this item. Existing console/OpenTelemetry/Aspire emission is the default unless the owner separately approves retention requirements and a backend.

### Implementation scope

- `src/LedgerLens.ServiceDefaults/` centralized exception-handler registration, safe structured logging, and Problem Details mapping
- Portfolio Core API endpoints that currently contain local exception translation
- `src/Services/MarketData/LedgerLens.MarketData.Api/Features/MarketDataEndpointExecution.cs`
- Typed Market Data unavailable outcomes at the Application/API boundary; `MarketDataUnavailableException` must not become a globally mapped CLR exception
- Focused middleware integration tests for `400`, `409`, `500`, `503`, localization, logging, correlation, non-disclosure, and cancellation
- Architecture enforcement preventing new endpoint-local broad exception catches or broad CLR-exception-to-status mappings
- `docs/plans/active/0004-unified-api-response-contract.md`
- `docs/architecture/OBSERVABILITY.md`
- `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md` when the failure-mapping rule changes
- `docs/project/PROJECT_STATE.md` only after verified behavior or project state materially changes

The configuration-validation catch in `FixedLocalUserOptionsValidator` is outside the HTTP request pipeline and must be reviewed separately. It must not be removed merely because endpoint exception handling becomes centralized.

### Acceptance criteria

- No admitted API endpoint contains a broad `try/catch` solely to translate an exception into an HTTP result.
- Current client-visible `400`, `409`, `500`, and `503` Problem Details shapes, localization, status codes, and stable error codes are preserved or deliberately updated with contract evidence.
- Every handled exception that requires an operational record produces exactly one structured log at the approved level.
- Logs correlate with the response through trace and correlation identifiers and contain no request body, credentials, connection strings, provider keys, provider payloads, personal email, or financial values.
- Client responses contain no raw exception message or stack trace.
- Expected request cancellation is not logged as an internal failure.
- Unexpected exceptions return generic `500` Problem Details and remain visible through the configured logging pipeline.
- No transitional global `ArgumentException -> 400` or `InvalidOperationException -> 409` mapping is introduced.
- Registered `IExceptionHandler` ordering and .NET 10 diagnostic-suppression behavior are explicit and produce neither missing nor duplicate unexpected-failure telemetry.
- Relevant integration, architecture, service, Gateway, localization/non-disclosure, and `git diff --check` gates pass.
- Runtime verification distinguishes emitted OpenTelemetry logs from durable retention; no persistence claim is made unless a retention backend is separately implemented and verified.

### Decision record

- **Decision:** Accepted Option B.
- **Owner rationale:** Expected business and validation failures must be explicit Application outcomes; broad exception types cannot safely define HTTP semantics.
- **Decision date:** 2026-09-24.
- **Implementation evidence:** Not started.
