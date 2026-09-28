# 0007.01 — Govern `201 Created` and `Location` per endpoint

Last reviewed: 2026-09-28.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: API contract and Gateway boundary

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [0007.02](02-typed-failures-and-exception-handling.md) — shares the API response and Problem Details boundary.
- [0007.04](04-collection-correctness-pagination-and-sorting.md) — changes public operation contracts and Gateway-follow tests.
- [0007.06](06-request-lifecycle-timeouts-mutations-and-exports.md) — asynchronous export status URLs and timeout failures also require truthful public routes.


**Status:** Accepted policy; endpoint decisions and implementation not started.

### Current behavior

The shared `ApiResults.Created(HttpContext, string location, T data)` helper returns `201 Created`, adds a caller-supplied `Location` header, and wraps the created representation in the shared `{ data, meta }` response envelope.

Current Portfolio Core create endpoints construct their own backend-relative location strings. For example, `InvestmentAccountsCreateEndpoint` returns:

```csharp
ApiResults.Created(
    httpContext,
    $"/v1/investment-accounts/get-one/{account.Id}",
    account);
```

The current source does not map `/v1/investment-accounts/get-one/{accountId}`, so that response advertises a location that cannot retrieve the created resource. The Gateway also exposes Portfolio Core below `/api/portfolio` and removes that prefix before forwarding. A backend-relative `/v1/...` location is therefore not automatically the canonical browser-facing `/api/portfolio/v1/...` location even when the backend route exists. Current create locations must be verified through the Gateway rather than only against backend route registration.

### Review concern

Whether Angular currently follows the header does not determine the HTTP contract. A truthful `Location` is useful for an independently addressable created resource, while a fabricated, internal-only, or workflow-only location is misleading. Adding a retrieval endpoint solely to manufacture a header would also violate YAGNI. The decision must therefore be made per create operation from resource semantics and a verified public retrieval route.

### Accepted policy

- Successful resource creation retains `201 Created` and the existing `{ data, meta }` representation.
- Decide `Location` independently for every create endpoint; there is no system-wide always/never rule.
- Retain `Location` only when the created object is independently addressable and an authorized canonical public retrieval route exists.
- Generate retained locations from centrally owned public route metadata or an equivalent Gateway-aware link policy. Do not concatenate backend paths in endpoints.
- Omit `Location` for transient workflow artifacts, append-only events, or resources without a demonstrated retrieval use case.
- Do not create `get-one` merely to satisfy the header.
- A retained location must be tested by following it through the Gateway and retrieving the same resource under the same ownership boundary.
- Returning `200 OK` merely to avoid `Location` remains rejected; a completed creation stays `201 Created`.

### Endpoint decision inventory

| Create operation | Current advertised location | Retrieval evidence | Decision |
| --- | --- | --- | --- |
| `PortfoliosCreate` | `/v1/portfolios/get-one/{id}` | Backend `PortfoliosGetOne` exists; public Gateway path still unverified | Pending endpoint decision |
| `InvestmentAccountsCreate` | `/v1/investment-accounts/get-one/{id}` | No current get-one route | Pending endpoint decision; do not add a route only for this header |
| `CashLedgerEntriesCreate` | `/v1/cash-ledger-entries/get-one/{id}` | No current get-one route; entry is part of an auditable ledger workflow | Pending endpoint decision |
| `SimulationAccountsCreate` | `/v1/simulation-accounts/get-one/{id}` | Backend `SimulationAccountsGetOne` exists; public Gateway path still unverified | Pending endpoint decision |
| `SimulationTradeDraftsCreate` | `/v1/simulation-trade-drafts/{id}` | No canonical operation segment or public get-one route | Pending endpoint decision; transient-draft semantics favor omission |

Each row must record `Retain` or `Omit`, owner rationale, public route evidence, decision date, and focused test evidence before its source changes.

### Implementation scope

- `src/LedgerLens.ServiceDefaults/ApiResults.cs`
- All current callers of `ApiResults.Created(...)` and public route metadata needed for retained locations
- Gateway integration coverage for response-header behavior and following retained locations
- Focused success-contract and endpoint tests
- `docs/plans/active/0004-unified-api-response-contract.md`
- `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md`
- `docs/project/PROJECT_STATE.md` only if verified project behavior or state materially changes

### Acceptance criteria

- Every admitted create endpoint returns `201` with the existing `{ data, meta }` body.
- No create response emits a fabricated, backend-internal, stale, unauthorized, or unverified `Location` header.
- Every retained `Location` resolves through the Gateway to the same owner-authorized resource.
- Every omitted `Location` has an endpoint-specific rationale and does not trigger creation of an otherwise unnecessary retrieval operation.
- Angular behavior remains unchanged except where a test previously asserted the header.
- Contract and architecture tests encode the newly accepted rule.
- Relevant .NET tests, Angular tests when affected, and `git diff --check` pass.

### Decision record

- **Decision:** Accepted per-endpoint policy; individual endpoint rows remain pending.
- **Owner rationale:** `Location` is useful only when it truthfully identifies an addressable public resource. Frontend usage alone neither requires nor forbids it.
- **Decision date:** 2026-09-24.
- **Implementation evidence:** Not started.
