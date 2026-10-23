# 0007.06 — Define request cancellation, timeout, mutation-disconnect, and export-job behavior

Last reviewed: 2026-09-28.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: Operational lifecycle and failure modes

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [0007.01](01-created-location-policy.md) — accepted asynchronous work needs canonical owner-authorized status and download URLs.
- [0007.02](02-typed-failures-and-exception-handling.md) — client cancellation, server timeout, dependency failure, and unexpected failure require distinct mapping and telemetry.
- [0007.04](04-collection-correctness-pagination-and-sorting.md) — reads and complete exports share filters, sorting, cancellation, and resource budgets.
- [0007.05](05-angular-feature-architecture-and-scss-system.md) — Angular feature data-access boundaries implement latest-request and lifecycle cancellation.
- [0007.07](07-persistence-port-decomposition.md) — mutation token handoff and transaction ownership must be explicit before store boundaries change.


**Status:** Accepted design direction; implementation not started. Timeout and synchronous/asynchronous export thresholds remain evidence gates.

### Current behavior

Current Minimal API endpoints accept `CancellationToken`, which ASP.NET Core binds to `HttpContext.RequestAborted`. Portfolio Core and Market Data handlers pass that token through Application ports to EF Core and provider `HttpClient` operations. A hard browser refresh or closed connection can therefore request cooperative cancellation of most current I/O.

This does not kill a process or thread, does not guarantee immediate termination of CPU-only work, and does not prove that a mutation rolled back. A database commit can complete before the disconnected client receives its response. Current Angular code also owns multiple manual subscriptions without a general superseded-request or component-destruction policy. No file-export endpoint, background export job, repository-owned request-timeout policy, end-to-end cancellation test, or export cleanup policy is currently implemented.

### Accepted operation policy

| Operation category | Browser refresh/disconnect | Required server behavior |
| --- | --- | --- |
| `GetOne`, `GetPage`, search, provider read | Cancel | Stop downstream EF/provider/CPU work cooperatively and promptly |
| Small synchronous PDF/CSV export | Cancel | Stop query/render/stream work and delete any temporary artifact |
| Large asynchronous export | Continue after accepted enqueue | Detach from request cancellation; expose owner-bound status and explicit cancellation |
| Mutation before durable transaction | Cancel | Stop before beginning the transaction when cancellation is already requested |
| Financial mutation after the atomic unit begins | Do not let client connectivity define the business outcome | Complete or roll back under a bounded server-operation token and expose an idempotent reconciliation path |
| Host shutdown | Cancel gracefully | Worker observes the host stopping token and records a safe interrupted/failed outcome where persistence exists |

Cancellation is cooperative. Every asynchronous boundary must accept and forward the appropriate token; long CPU loops must check it at coarse, deterministic boundaries. Expected client cancellation is not an application error and must not produce a user error toast or an Error log. Server timeout, host shutdown, explicit job cancellation, dependency timeout, and client disconnect remain distinguishable outcomes in low-cardinality telemetry.

### Read and Angular cancellation rules

- Preserve the endpoint `CancellationToken -> handler -> Application port -> EF/HttpClient` chain and add architecture/static coverage that detects newly dropped tokens at admitted I/O boundaries.
- Add `CancellationToken.ThrowIfCancellationRequested()` or equivalent checks around expensive CPU phases where no cancellable API is awaited; do not add checks inside tiny inner loops when the overhead is not justified.
- Angular route/filter/page/sort request streams use `switchMap` or an equivalent latest-request policy. Component-owned imperative subscriptions use `takeUntilDestroyed` or an equally explicit lifecycle boundary.
- A stale response cannot replace a newer selected resource, sort, filter, page, quote, candle series, valuation, or error state.
- Expected abort/cancellation does not call the ordinary failure UI path.
- Do not attempt to write Problem Details after the client connection is gone or a file response has started.

### Request timeout policy

ASP.NET Core does not impose the required application timeout categories automatically. Introduce named request-timeout policies for at least interactive local reads, external-provider reads, synchronous export generation, and ordinary mutations. Do not apply one arbitrary global duration to every endpoint.

Exact duration values are blocked pending representative measurements and provider constraints. The evidence for each value must include operation category, dataset/profile, cold/warm state, run count, percentile, downstream timeout, and cleanup behavior. The configured dependency timeout must leave enough outer-request budget for safe translation/cleanup. Background jobs use their own per-job deadline rather than an HTTP request timeout.

### Mutation disconnect and idempotency policy

- Validate transport, authentication/authorization, ownership, and expected failures before starting a durable transaction.
- If request cancellation is already signaled before the transaction begins, do not start the mutation.
- Once an admitted financial atomic unit begins, use a bounded server-operation token that is not controlled solely by browser connectivity. Host shutdown and the operation deadline still apply.
- Every mutation that a client can reasonably retry after losing the response must have an explicit idempotency contract, stable client/request operation identifier, or a deterministic resource/status reconciliation path.
- A client that loses the response reconciles through `get-one` or operation status before retrying. UI copy must not claim failure solely because the response was lost.
- Tests must cover cancellation before transaction start, cancellation during work, commit followed by response loss, duplicate retry, rollback, and the absence of partial ledger/correction state.

This item does not retroactively declare all current mutations idempotent. Each operation records its strategy before changing cancellation behavior.

### Synchronous file export policy

- `export-file` returns a file response, never the success JSON envelope or page metadata.
- The default export scope is the complete owner-authorized filtered/sorted result. `Current page` is a separate explicit request/user choice.
- Complete authentication, authorization, ownership, request validation, filename normalization, and inexpensive preflight checks before starting the response.
- Pass cancellation through query, renderer, compression, temporary-file, and response-stream operations.
- Temporary exports live only in the approved protected application-data location outside the repository and are removed in `finally` on success, cancellation, timeout, or failure.
- After response headers/body start, a failure aborts the stream and records a safe diagnostic; it does not replace the response with JSON.
- PDF generation has bounded input/assets/font behavior and a measured memory/time limit. Any CSV implementation prevents spreadsheet formula injection for cells beginning with formula-control characters.
- Do not buffer an unbounded export in process memory. Use streaming or a bounded temporary artifact according to measured renderer capabilities.

### Asynchronous export policy

When representative evidence shows that an export cannot reliably meet the synchronous budget, use an asynchronous job rather than extending the HTTP request indefinitely:

1. Validate and authorize the export request.
2. Persist/enqueue an immutable owner-bound job specification and return `202 Accepted` with a job identifier and canonical status URL.
3. After successful enqueue, do not retain `HttpContext`, scoped request services, or `RequestAborted`; the worker creates its own dependency scope.
4. Process under host-shutdown, per-job deadline, and explicit user-cancel tokens.
5. Expose `Queued`, `Running`, `Completed`, `Failed`, `Cancelled`, and `Expired` states without leaking internal errors.
6. Authorize status, cancel, and download against the same owner/resource boundary as generation.
7. Store output outside the repository with expiry, quota, cleanup, audit, and privacy controls.
8. Refreshing or closing the browser does not cancel an accepted job; only explicit cancel, deadline, shutdown, or failure changes it.

Do not add a queue product or durable job store until the first asynchronous export use case and reliability requirements justify that dependency.

### Implementation scope

- Shared ServiceDefaults request-timeout and cancellation classification
- Gateway-to-service cancellation integration coverage
- Portfolio Core and Market Data cancellation propagation and CPU checkpoints
- Angular API/data-access request lifecycle behavior introduced with Milestone 5A
- Future export endpoints, renderer/storage boundary, background worker/job state only when an export milestone is separately accepted
- `docs/architecture/DOTNET_APPLICATION_ARCHITECTURE.md`, `SECURITY_AND_PRIVACY.md`, `OBSERVABILITY.md`, `API_AND_FEATURE_NAMING_STANDARD.md`, quality/performance documents, and project state after verified behavior changes

### Acceptance criteria

- Cancelling a request through the Gateway is observed by the backend handler and its active EF/provider operation.
- Superseded Angular requests and destroyed components stop applicable work and cannot update current UI state.
- Expected client cancellation produces neither generic `500` Problem Details nor an Error log.
- Server timeout and client cancellation are distinguishable; configured timeout behavior is verified without a debugger.
- CPU-heavy calculation/export phases stop within a measured bound after cancellation without changing deterministic results when not cancelled.
- Synchronous export cancellation removes temporary artifacts and does not continue rendering after disconnect.
- An accepted asynchronous export survives browser refresh, supports explicit owner-authorized cancellation, and expires/cleans its output.
- Mutation tests prove atomicity and the documented idempotency/reconciliation result when the response is lost.
- No implementation claims that browser refresh guarantees rollback or that cancellation kills a process/thread.

### Decision record

- **Decision:** Accepted design direction.
- **Owner rationale:** Abandoned reads and synchronous exports must stop wasting resources, while accepted background jobs and financial mutations need explicit lifecycle, atomicity, idempotency, and reconciliation semantics independent of browser connectivity.
- **Decision date:** 2026-09-24.
- **Implementation evidence:** Not started; exact timeout/export thresholds require measured evidence.
