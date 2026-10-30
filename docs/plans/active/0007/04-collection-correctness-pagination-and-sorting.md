# 0007.04 — Require server-side pagination and sorting for pageable resource collections

Last reviewed: 2026-10-30.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: Financial correctness and collection contracts

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [0007.02](02-typed-failures-and-exception-handling.md) — page and sort validation failures use the shared typed-failure contract.
- [0007.03](03-semantic-naming-consistency.md) — operation renames must reflect cardinality without becoming a repository-wide rename.
- [0007.05](05-angular-feature-architecture-and-scss-system.md) — the Simulation page contract is implemented only after the feature extraction boundary exists.
- [0007.06](06-request-lifecycle-timeouts-mutations-and-exports.md) — paged queries, complete-ledger calculations, and exports require cancellation and measured resource bounds.
- [0007.07](07-persistence-port-decomposition.md) — query shapes and store boundaries must stabilize before persistence ports split.


**Status:** In implementation. The complete-ledger cap was removed in source; the over-5,000-entry PostgreSQL regression test has been added but has not passed the distributed runtime gate yet. Pagination and sorting work has not started.

### Current behavior

The shared success envelope already distinguishes ordinary list responses from paged responses. `ApiResults.Ok(...)` returns `{ data, meta }` without pagination, while `ApiResults.Page(...)` adds `pageNumber`, `pageSize`, `totalCount`, and `totalPages` under `meta.pagination`. Sorting metadata is not currently part of the response contract.

Current resource collections are inconsistent. For example, `SimulationAccountsGetList` accepts only `portfolioId`, returns an `IReadOnlyList<SimulationAccountListItem>`, and silently limits the Infrastructure query with `.Take(50)`. A caller cannot select a page or ordering and cannot determine whether additional rows exist. The value `50` is a defensive magic number rather than a documented Simulation Account business limit.

The problem is wider than operation names. Current Portfolio Core source also contains:

- `PortfoliosGetListAsync(...).Take(100)`, which silently truncates a browsable root resource collection.
- `PortfoliosGetOneAsync(...)` loading at most 50 investment accounts and at most 500 recent cash-ledger entries across those accounts. The nested response provides no truncation metadata, and a global 500-entry window can leave one account with no displayed entries even when that account has older data.
- `GetSimulationEntriesAsync(...).Take(5000)`, whose result feeds position rebuilding, oversell validation, trade correction, current valuation, historical valuation, and Simulation Account overview. This is not a page or UI preview: ignoring entries after the first 5,000 can change financial results.
- Provider/test-specific limits such as bounded autocomplete/search candidates, which may be valid but still require an explicit category, owner, and test.

An inventory based only on `GetList`/`SearchList` names would therefore miss nested arrays, calculation inputs, projections, provider limits, and other silently bounded collections.

The architecture already permits two safe-read transports: `GET` for bounded scalar query parameters and HTTP `QUERY` for structured request content. The number of request properties alone does not determine the method; their structure and compatibility requirements do.

### Accepted decision

Every endpoint that retrieves a pageable collection of resource records must use server-side pagination and sorting. It must expose a `GetPage` or `SearchPage` operation, accept the shared page/sort contract, return a typed page result, and include both effective pagination and sorting metadata in every successful response.

Use `GET .../get-page` when identifiers, flat filters, page number, page size, and one admitted sort fit naturally in the URI query string. Use HTTP `QUERY .../search-page` with an explicit JSON `Content-Type` only when the search requires structured content such as nested filters, multiple ranges, large identifier sets, or multi-column sorting. Do not switch from `GET` to `QUERY` merely because a flat request has several properties.

This requirement applies to browsable resource collections backed by server-owned data. It does not force pagination onto `GetOne`, create/update/delete commands, finite combobox or autocomplete results, caller-bounded batch lookups, candles/time-series observations, or calculation/command results whose array is one atomic business result. Any new non-paged collection category requires an explicit naming-standard entry and architecture-test coverage rather than an incidental `.Take(...)` limit.

`export-file` is also exempt from the JSON page-response contract. A PDF/CSV response is a binary/stream response with the correct `Content-Type` and safe `Content-Disposition`; it does not use `{ data, meta.pagination }`. By default an export covers the complete authorized filtered/sorted result. Exporting only the current UI page requires an explicit user-visible `Current page` scope rather than inheriting page state silently. Resource limits, synchronous versus asynchronous execution, cancellation, storage, expiry, and cleanup are governed by Item 6.

### Correctness prerequisite — complete Simulation ledger input

Before structural pagination work begins, remove or replace the silent 5,000-entry cap from every calculation path. Simulation calculations must process the complete owner-authorized ledger in deterministic `EffectiveAt`, `RecordedAt`, and `Id` order.

The smallest safe first correction is to remove the arbitrary cap and add a regression fixture with more than 5,000 entries. If representative benchmarks then show unacceptable allocation or latency, introduce an ordered streaming/incremental fold without changing calculation results. Do not introduce a business maximum unless the product explicitly approves it and the same limit is enforced on mutation before data exceeds it.

### Required contracts

The HTTP request model must carry at least:

```csharp
public sealed record PageRequest(
    int PageNumber,
    int PageSize,
    string SortBy,
    SortDirection SortDirection);
```

For `GET`, Minimal API endpoints bind the flat members from the query string through one composed parameter contract. For HTTP `QUERY`, the same logical page and sort values are carried in a structured JSON request contract. Transport binding types and raw `string SortBy` values remain outside Domain, persistence, and shared Application paging types.

The endpoint validates `sortBy` against its operation-specific allowlist and maps it to a typed Application sort value, for example `SimulationAccountsSortField.CreatedAt`. Application and Infrastructure must not receive an arbitrary property name or use reflection/dynamic SQL to sort.

The Application result must be typed rather than returning a bare collection:

```csharp
public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int PageNumber,
    int PageSize);
```

The operation query retains its typed effective sort field/direction, and the API maps that typed value back to the stable public metadata name. Do not create a single global sort enum containing unrelated feature fields.

The HTTP success response keeps the item array in `data`. Its required metadata shape is:

```json
{
  "data": [],
  "meta": {
    "correlationId": "...",
    "pagination": {
      "pageNumber": 1,
      "pageSize": 50,
      "totalCount": 125,
      "totalPages": 3
    },
    "sorting": {
      "sortBy": "createdAt",
      "sortDirection": "desc"
    }
  }
}
```

`meta.pagination` and `meta.sorting` are required for every successful `GetPage` and `SearchPage` response, including an empty page. They remain absent from non-paged responses.

### Validation and query rules

- Require `pageNumber >= 1` and a `pageSize` within the centrally documented minimum and maximum; the implementation candidate maximum is `100`, subject to verification against performance budgets.
- Require `sortBy` and `sortDirection`; accept only stable wire values and return localized `400` Problem Details for invalid input.
- Define a per-operation allowlist that maps public sort names to explicit expressions. Do not reflect over arbitrary property names or concatenate client input into SQL.
- Define case/collation, null placement, date/time interpretation, and normalization for each admitted sort field so ordering is stable across database, API, and Angular.
- Apply filtering first, obtain `LongCountAsync`, apply the requested ordering, add a unique deterministic tie-breaker such as `Id`, and only then apply `Skip`/`Take` and projection.
- Validate page-offset arithmetic so an excessive page number cannot overflow the EF Core offset.
- Echo the normalized effective page and sorting values in response metadata so the client does not have to infer server defaults or normalization.
- Remove defensive collection magic numbers such as `.Take(50)` once the operation is migrated. A true business maximum must instead be named, documented, enforced on mutation, and tested independently from pagination.
- Return `200 OK` with empty `data` plus the requested/effective page metadata for a valid out-of-range page. After concurrent deletion, Angular may move back one page once when the current non-first page becomes empty.
- Treat count and row retrieval as an eventually consistent UI read under ordinary isolation; do not add a transaction solely to make page metadata a snapshot unless a demonstrated business invariant requires it.
- Verify the filter/sort/tie-breaker query shape against representative data and the existing index set before proposing an index. Record filter columns, sort columns, selectivity, index order, and write cost for every new index.
- Exercise deep offsets against the performance budget. If page-number pagination cannot meet the admitted bound, open a separate cursor-pagination decision rather than changing the Version 1 contract silently.

### Enforcement

- Shared request/result contracts make missing page and sort data visible in handler signatures and return types.
- `ApiResults.Page(...)` accepts the typed page result and is the only admitted success helper for `GetPage` and `SearchPage`.
- Architecture tests verify operation naming, paged handler result types, required response metadata, and the absence of newly introduced pageable `GetList`/`SearchList` routes.
- Focused contract tests cover ordinary, final, out-of-range, and empty pages; ascending and descending sorts; invalid fields/directions; maximum page size; deterministic ordering when primary sort values tie; and 64-bit counts.
- Persistence/integration tests prove that filtering, counting, ordering, `Skip`, and `Take` execute server-side and that owner/authorization predicates are applied before counting and materialization.
- Angular clients always send explicit page and sort values for paged operations and consume both metadata blocks from the response.
- Any admitted HTTP `QUERY` route must pass the existing Angular-to-Gateway-to-service compatibility gate, including body forwarding, JSON `Content-Type`, CORS preflight, observability, OpenAPI representation, and generated-client behavior.

### Initial migration scope

- Replace `SimulationAccountsGetList` with `SimulationAccountsGetPage` across route, OperationKey, API/Application feature folders, handler/store contracts, Angular client, and tests.
- Inventory every current `GetList` and `SearchList` operation and classify it as a pageable resource collection or an explicitly admitted non-paged category before renaming or changing behavior.
- Inventory every `.Take`, `Skip`, SQL `LIMIT`, returned collection, and nested collection-bearing response in production code. Record whether it is pageable, finite lookup, caller-bounded batch, atomic calculation input/result, or explicitly named preview.
- Migrate `PortfoliosGetList` to `PortfoliosGetPage` unless an approved finite portfolio limit is enforced on creation.
- Replace the unbounded/truncated child graph in `PortfoliosGetOne` with independently pageable `InvestmentAccountsGetPage(portfolioId)` and `CashLedgerEntriesGetPage(accountId)` operations. During an incremental migration, any retained preview must be named as a preview and disclose `totalCount`/`hasMore`; it must not appear complete.
- Add shared page, sort, page-result, pagination-metadata, and sorting-metadata contracts in their correct transport/Application ownership layers without introducing a mediator, runtime dispatcher, reflection scanning, or generic repository.
- Update `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md`, `docs/architecture/DOTNET_APPLICATION_ARCHITECTURE.md`, plan 0004, affected feature documentation, and `docs/project/PROJECT_STATE.md` only after behavior is implemented and verified.

### Required Angular changes for the initial migration

The `SimulationAccountsGetList` migration is not backend-complete until the Angular request, response, state, UI, and tests are migrated in the same implementation change. At minimum, update:

- `web/ledgerlens-web/src/app/api-routes.ts`: replace `SimulationAccountsGetList` with `SimulationAccountsGetPage` and require explicit `pageNumber`, `pageSize`, `sortBy`, and `sortDirection` query parameters with safe URI encoding.
- `web/ledgerlens-web/src/app/api-response.ts`: add sorting metadata and a typed way to retain both `meta.pagination` and `meta.sorting`; do not unwrap `data` and discard page metadata for paged operations.
- `web/ledgerlens-web/src/app/app.ts`: replace the bare-array load with a paged request, retain the effective page/sort state, handle empty and out-of-range pages, and reload deterministically after creating or selecting a Simulation Account.
- Store page, sort, filter, and selected resource identifiers that must survive refresh/back/forward in route and query parameters rather than only component memory.
- Cancel superseded page/sort/filter requests with `switchMap` or an equivalent mechanism, and prevent a stale response from overwriting newer state.
- After creation, select the returned resource by ID and refresh the current page independently; do not assume the new item belongs on the current page under every sort.
- `web/ledgerlens-web/src/app/app.html`: add accessible paging controls and visible sort controls when more than one page or user-selectable sorting is available; preserve keyboard operation and clear disabled/current-page states.
- `web/ledgerlens-web/src/app/api-routes.spec.ts`: assert the canonical `get-page` route and every required encoded page/sort parameter.
- `web/ledgerlens-web/src/app/api-response.spec.ts` and `web/ledgerlens-web/src/app/app.spec.ts`: cover pagination/sorting metadata parsing, page navigation, sort changes, empty pages, API failures, and post-create reload behavior.

The Angular production build, focused component/contract tests, desktop layout, mobile layout, keyboard focus, and document-overflow checks are required acceptance evidence. A backend-only change or a client change that continues to assume a bare array does not satisfy this item.

### Acceptance criteria

- No pageable resource endpoint returns a silently truncated bare list.
- No atomic financial calculation silently truncates its input. A regression scenario with more than 5,000 Simulation entries produces the same complete-ledger result as the unbounded calculation.
- Every production collection limit is classified and either represented in a page/preview contract, enforced as a real business limit, caller-bounded, or removed.
- Every `GetPage` and `SearchPage` request supplies validated pagination and sorting, and every successful response includes matching `meta.pagination` and `meta.sorting`.
- `GET` remains the transport for flat page retrieval; HTTP `QUERY` is used only for structured search after the full compatibility gate passes.
- Sorting uses an explicit per-operation allowlist and a unique deterministic tie-breaker.
- Counts and page rows preserve the same filters and authorization boundary and execute server-side.
- Sort field semantics, null placement, collation/casing, deterministic tie-breaker, and representative query performance are documented and tested per operation.
- Existing non-paged collection operations are retained only when they match a documented exception category.
- The Angular route builder, response contracts, state, paging/sort controls, and focused tests are migrated with the backend; page metadata is not discarded during response unwrapping.
- Backend, Angular, architecture, contract, integration, privacy, and `git diff --check` gates pass without changing financial calculations or leaking user data.

### Decision record

- **Decision:** Accepted.
- **Owner rationale:** Pageable resource collections must not rely on hidden hard-coded row limits. Pagination and sorting must be explicit, server-side, and present in both request and response contracts. `GET` remains appropriate for flat page parameters; HTTP `QUERY` is reserved for genuinely structured searches rather than selected solely because the property count grows.
- **Decision date:** 2026-09-22.
- **Implementation evidence:** `PortfolioCoreStore.GetSimulationEntriesAsync` no longer applies `.Take(5000)` and retains the owner predicate and deterministic `EffectiveAt`, `RecordedAt`, `Id` order. A synthetic 5,001-entry Aspire/PostgreSQL test now checks complete position quantity and cross-owner isolation. The Infrastructure build passed with zero warnings/errors on 2026-10-30; the distributed test remains unverified.
