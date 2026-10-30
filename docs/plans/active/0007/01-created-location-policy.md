# 0007.01 — Govern `201 Created` and `Location` per endpoint

Last reviewed: 2026-10-25.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: API contract and Gateway boundary

This decision record tracks the accepted policy and its implementation evidence.

## Related decisions

- [0007.02](02-typed-failures-and-exception-handling.md) — shares the API response and Problem Details boundary.
- [0007.04](04-collection-correctness-pagination-and-sorting.md) — changes public operation contracts and Gateway-follow tests.
- [0007.06](06-request-lifecycle-timeouts-mutations-and-exports.md) — asynchronous export status URLs and timeout failures also require truthful public routes.


**Status:** Implemented and verified through the Gateway against the real Portfolio Core service.

### Previous behavior

The shared `ApiResults.Created(HttpContext, string location, T data)` helper returns `201 Created`, adds a caller-supplied `Location` header, and wraps the created representation in the shared `{ data, meta }` response envelope.

Previously, Portfolio Core create endpoints constructed backend-relative location strings. For example, `InvestmentAccountsCreateEndpoint` returned:

```csharp
ApiResults.Created(
    httpContext,
    $"/v1/investment-accounts/get-one/{account.Id}",
    account);
```

The source has no `/v1/investment-accounts/get-one/{accountId}` mapping, so the old response advertised a location that could not retrieve the created resource. The Gateway exposes Portfolio Core below `/api/portfolio` and removes that prefix before forwarding. The old backend-relative `/v1/...` location was therefore not the canonical browser-facing `/api/portfolio/v1/...` location even when the backend route existed.

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
| `PortfoliosCreate` | `/v1/portfolios/get-one/{id}` | `PortfoliosGetOne` maps the owner-checked resource route | Retain; Gateway exposes `/api/portfolio/v1/portfolios/get-one/{id}` |
| `InvestmentAccountsCreate` | `/v1/investment-accounts/get-one/{id}` | No current get-one route | Omit; account creation still returns `201` and its representation |
| `CashLedgerEntriesCreate` | `/v1/cash-ledger-entries/get-one/{id}` | No current get-one route | Omit; append-only entry remains available through existing ledger workflows |
| `SimulationAccountsCreate` | `/v1/simulation-accounts/get-one/{id}` | `SimulationAccountsGetOne` maps the owner-checked resource route | Retain; Gateway exposes `/api/portfolio/v1/simulation-accounts/get-one/{id}` |
| `SimulationTradeDraftsCreate` | `/v1/simulation-trade-drafts/{id}` | No canonical get-one route | Omit; the transient draft is consumed by workflow operations |

Endpoint decisions were confirmed by the owner in the 0007.01 design discussion before implementation. The three omissions require no new retrieval route.

### Implemented boundary

- Direct Portfolio Core responses keep service-local `/v1/...` locations for the two retained operations. The Gateway is the supported public entrypoint and rewrites only `201 Created` Portfolio response locations to origin-relative `/api/portfolio/v1/...`, deriving its prefix and route identity from `GatewayRoutes.Definitions`.
- `ApiResults.Created(HttpContext, T)` returns the same `{ data, meta }` body and `201` without a `Location` header for the three omitted operations.
- The Gateway removes a malformed, absolute, wrong-root, encoded, query-bearing, fragment-bearing, or otherwise unsafe created location and logs a structured warning without logging the supplied header value. It preserves the upstream `201` and body because the mutation may already have committed.
- Angular continues to unwrap `data` and does not depend on `Location`.

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

- **Decision:** Retain for `PortfoliosCreate` and `SimulationAccountsCreate`; omit for the other three current Portfolio Core create endpoints. Gateway owns the public rewrite.
- **Owner rationale:** `Location` is useful only when it truthfully identifies an addressable public resource. Frontend usage alone neither requires nor forbids it.
- **Decision date:** 2026-09-28 design discussion; source implemented 2026-10-23.
- **Implementation evidence (2026-10-23):** Gateway integration tests passed `13/13` against a synthetic Portfolio service, covering public header rewrite and follow, invalid header removal, and the headerless Created envelope. Architecture tests passed `20/20`, including the five-endpoint retain/omit matrix. Gateway, Portfolio Core API, and distributed test project builds passed with zero warnings/errors; the AppHost resource-model test passed `1/1`; documentation consistency and `git diff --check` passed.
- **Distributed verification (2026-10-25):** On `codex/0007-01-created-location-policy`, both explicitly enabled Aspire distributed tests passed `2/2` with ephemeral PostgreSQL and NATS. The create scenario asserted the header decisions for all five create operations, then followed both retained public Gateway locations to the same owner-authorized Portfolio and Simulation Account resources.
