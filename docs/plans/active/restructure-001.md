# Restructure 001 — Code Review Decision Log

Last reviewed: 2026-09-22. Status: In review; no listed item is approved for implementation until the owner records a decision.

## Goal

Collect narrowly scoped restructuring candidates discovered during the owner's code review. Each item records the current behavior, evidence, options, expected impact, and an explicit owner decision before source changes begin.

## Review workflow

1. Review one numbered item at a time against the current source.
2. Record the owner decision as `Accepted`, `Rejected`, or `Deferred`; the default is `Pending`.
3. Do not treat discussion, an initial preference, or inclusion in this plan as implementation approval.
4. For an accepted item, update the affected source, tests, and governing documentation together.
5. Verify the smallest relevant build and tests, then record exact evidence under that item.

## 1. Decide whether create responses should omit the `Location` header

**Status:** Pending owner decision.

### Current behavior

The shared `ApiResults.Created(HttpContext, string location, T data)` helper returns `201 Created`, adds a caller-supplied `Location` header, and wraps the created representation in the shared `{ data, meta }` response envelope.

Current Portfolio Core create endpoints construct their own location strings. For example, `InvestmentAccountsCreateEndpoint` returns:

```csharp
ApiResults.Created(
    httpContext,
    $"/v1/investment-accounts/get-one/{account.Id}",
    account);
```

The current source does not map `/v1/investment-accounts/get-one/{accountId}`, so that response advertises a location that cannot currently retrieve the created resource. Other create endpoints must be reviewed individually rather than assuming every advertised location is valid.

### Review concern

The Angular client already owns its next navigation or API-call decision and receives the new resource identifier in the response body. Supplying a backend-generated follow-up URL may therefore add route coupling without a demonstrated client requirement. An invalid or stale `Location` header is worse than omitting an optional header.

### Options

#### Option A — retain `Location`

- Keep `201 Created` with a valid canonical resource URL.
- Add or retain a corresponding retrievable endpoint for every advertised location.
- Prefer route-name-based URL generation if endpoint paths must be refactor-safe.
- Preserve the current plan 0004 and API naming-standard requirements.

#### Option B — omit `Location` and retain `201 Created`

- Return the created representation and correlation metadata with status `201`.
- Let the Angular client decide what route or operation to use next.
- Replace the shared helper signature with `ApiResults.Created(HttpContext, T data)`.
- Remove caller-constructed location strings from all admitted create endpoints.
- Update plan 0004, the API naming standard, and contract tests so they no longer require `Location`.

Proposed endpoint shape if this option is accepted:

```csharp
return account is null
    ? problems.NotFound(httpContext)
    : ApiResults.Created(httpContext, account);
```

Proposed helper behavior:

```csharp
public static IResult Created<T>(HttpContext httpContext, T data) =>
    Results.Json(
        CreateResponse(httpContext, data),
        statusCode: StatusCodes.Status201Created);
```

#### Option C — return `200 OK`

- Not recommended merely to avoid `Location`; successful creation still has the more precise `201 Created` status.
- Consider only if the operation is later redefined so it no longer creates a resource.

### Initial review position

The owner currently prefers Option B because the frontend should know and choose its next request. This records the discussion only; the item remains `Pending` until the owner explicitly accepts, rejects, or defers it.

### Implementation scope if Option B is accepted

- `src/LedgerLens.ServiceDefaults/ApiResults.cs`
- All current callers of `ApiResults.Created(...)`
- Focused success-contract and endpoint tests
- `docs/plans/active/0004-unified-api-response-contract.md`
- `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md`
- `docs/project/PROJECT_STATE.md` only if verified project behavior or state materially changes

### Acceptance criteria if Option B is accepted

- Every admitted create endpoint returns `201` with the existing `{ data, meta }` body.
- No create response emits a fabricated, stale, or unverified `Location` header.
- Angular behavior remains unchanged except where a test previously asserted the header.
- Contract and architecture tests encode the newly accepted rule.
- Relevant .NET tests, Angular tests when affected, and `git diff --check` pass.

### Decision record

- **Decision:** Pending.
- **Owner rationale:** Pending.
- **Decision date:** Pending.
- **Implementation evidence:** Not started.

## 2. Centralize API exception handling and structured logging

**Status:** Pending owner decision.

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

### Initial review position

The owner currently prefers exception handling and logging in middleware before endpoint execution completes, with unnecessary `try/catch` blocks removed from endpoints and other application code. Option C is the initial implementation candidate because it can preserve current HTTP behavior while making the unsafe broad mappings visible and removable. This records the discussion only; the item remains `Pending` until the owner explicitly accepts, rejects, or defers it.

### Proposed logging policy for review

- Expected `4xx` outcomes: structured `Warning` only when an exception remains necessary during transition; include stable error code, exception type, operation/route, trace ID, and correlation ID, but not raw request bodies or raw exception messages.
- Dependency/provider `503` outcomes: structured `Error` with stable error code, provider identifier when allowlisted, trace ID, and correlation ID; exclude provider payloads, request URLs containing keys, and credentials.
- Unexpected `500` outcomes: structured `Error` from the centralized handler with exception type, safe diagnostic context, trace ID, and correlation ID; return only generic localized Problem Details to the client.
- Expected request cancellation: do not emit an application error log when `HttpContext.RequestAborted` caused the cancellation.
- Do not add a database table, Seq, Loki, or another durable logging product in this item. Existing console/OpenTelemetry/Aspire emission is the default unless the owner separately approves retention requirements and a backend.

### Implementation scope if Option C is accepted

- `src/LedgerLens.ServiceDefaults/` centralized exception-handler registration, safe structured logging, and Problem Details mapping
- Portfolio Core API endpoints that currently contain local exception translation
- `src/Services/MarketData/LedgerLens.MarketData.Api/Features/MarketDataEndpointExecution.cs`
- Market Data-specific unavailable-exception mapping when it cannot remain service-independent
- Focused middleware integration tests for `400`, `409`, `500`, `503`, localization, logging, correlation, non-disclosure, and cancellation
- Architecture enforcement preventing new endpoint-local broad exception catches without an allowlisted reason
- `docs/plans/active/0004-unified-api-response-contract.md`
- `docs/architecture/OBSERVABILITY.md`
- `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md` when the failure-mapping rule changes
- `docs/project/PROJECT_STATE.md` only after verified behavior or project state materially changes

The configuration-validation catch in `FixedLocalUserOptionsValidator` is outside the HTTP request pipeline and must be reviewed separately. It must not be removed merely because endpoint exception handling becomes centralized.

### Acceptance criteria if Option C is accepted

- No admitted API endpoint contains a broad `try/catch` solely to translate an exception into an HTTP result.
- Current client-visible `400`, `409`, `500`, and `503` Problem Details shapes, localization, status codes, and stable error codes are preserved or deliberately updated with contract evidence.
- Every handled exception that requires an operational record produces exactly one structured log at the approved level.
- Logs correlate with the response through trace and correlation identifiers and contain no request body, credentials, connection strings, provider keys, provider payloads, personal email, or financial values.
- Client responses contain no raw exception message or stack trace.
- Expected request cancellation is not logged as an internal failure.
- Unexpected exceptions return generic `500` Problem Details and remain visible through the configured logging pipeline.
- Transitional mappings are explicitly enumerated and have a documented deletion condition tied to typed expected-failure coverage.
- Relevant integration, architecture, service, Gateway, localization/non-disclosure, and `git diff --check` gates pass.
- Runtime verification distinguishes emitted OpenTelemetry logs from durable retention; no persistence claim is made unless a retention backend is separately implemented and verified.

### Decision record

- **Decision:** Pending.
- **Owner rationale:** Pending.
- **Decision date:** Pending.
- **Implementation evidence:** Not started.

## 3. Establish system-wide semantic naming consistency

**Status:** Pending owner decision and system-wide assessment.

### Current evidence

`SimulationAccountsCreateHandler.ToListItem(SimulationAccount account)` maps one domain entity to a `SimulationAccountListItem`. The word `ListItem` describes the DTO's original presentation role rather than the cardinality of the method result, so the method does not create a list. The same mapper and DTO are also reused by `SimulationAccountsGetOneHandler` to build a single-account overview. This makes both the type name and method name narrower than their actual responsibilities, while placing a shared mapping function under the create operation's handler.

The current naming standard also states that C# methods, parameters, and local variables use `camelCase`. That rule conflicts with standard .NET casing, where method names use `PascalCase` while parameters and local variables use `camelCase`. This must be resolved as part of the wider assessment rather than copied into new code.

### Review concern

Renaming only `ToListItem` would correct one symptom while leaving the same ambiguity available elsewhere. Classes, records, interfaces, methods/functions, parameters, local variables, folders, files, API operation keys, Angular identifiers, and shared mapping helpers need one coherent naming direction across the system.

A name must describe the identifier's actual business meaning, responsibility, result shape, and cardinality. It must not imply list, page, single-item selection, creation, persistence, or another behavior that the identifier does not perform. Reused types and functions must be named for their shared meaning and owned by a shared cohesive location rather than by one incidental caller.

This assessment is semantic, not merely mechanical casing cleanup. It must review related identifiers together so a rename does not create new inconsistencies across API, Application, Domain, Infrastructure, Angular, tests, and documentation.

### Assessment scope

- Inventory production C# and TypeScript identifiers, prioritizing public types, shared DTOs, handler and store methods, mappers, API contracts, Angular API methods, and names reused by multiple operations.
- Compare class/type nouns, method/function verbs, result types, cardinality, side effects, and ownership against actual behavior and call sites.
- Identify misleading role suffixes such as `ListItem`, `Manager`, `Helper`, `Data`, or `Info` when the implementation has a broader, narrower, or different responsibility.
- Verify that operation-specific identifiers follow the accepted `OperationKey`, while genuinely shared concepts use stable domain vocabulary rather than an arbitrary operation name.
- Correct the C# casing rule so types and methods use `PascalCase`, parameters and local variables use `camelCase`, and standard .NET acronym casing remains consistent.
- Review filenames, namespaces, folders, tests, architecture rules, and documentation together with each accepted rename.
- Exclude generated code, EF migrations, snapshots, external protocol fields, and persisted schema identifiers unless a separately reviewed compatibility or migration plan explicitly includes them.
- Preserve public API JSON fields, routes, database schema, and other external contracts unless the owner explicitly approves a contract migration.

### Options

#### Option A — fix only the reported simulation mapper

- Rename `SimulationAccountListItem` and `ToListItem` locally and move the mapper to a shared owner.
- Smallest immediate diff, but it does not prevent equivalent ambiguous naming elsewhere.

#### Option B — perform a governed system-wide naming assessment and staged correction

- First produce an evidence-backed inventory of ambiguous or inconsistent identifiers across the admitted production code.
- Define the intended domain vocabulary, semantic naming rules, casing rules, ownership rules, and compatibility boundaries before source renames.
- Group accepted renames into cohesive, independently verifiable stages rather than one unreviewable repository-wide rename.
- Add focused architecture/static checks where a rule can be enforced reliably without rejecting legitimate domain names.
- Use `SimulationAccountListItem` / `ToListItem` as the first recorded example, not as the complete scope.

### Initial review position

The owner requires the assessment to cover naming consistency throughout the system and explicitly rejects a local-only correction. Option B is therefore the current candidate. Inclusion in this plan records that direction but does not authorize implementation until the inventory, compatibility impact, and staged rename set have been reviewed.

### Implementation scope if Option B is accepted

- Current production C# and Angular/TypeScript source, excluding the explicit compatibility exclusions above
- Direct call sites, request/response types, mappers, tests, filenames, namespaces, and feature folders affected by each accepted rename
- `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md`
- Architecture/static naming tests where rules can be expressed without brittle keyword bans
- Relevant active plans and project documentation that reference renamed identifiers
- `docs/project/PROJECT_STATE.md` only after verified behavior or project state materially changes

### Acceptance criteria if Option B is accepted

- The assessment covers the admitted production system rather than only `SimulationAccountListItem` and `ToListItem`.
- Every accepted class/type and method/function name matches its actual domain meaning, responsibility, result shape, cardinality, side effects, and ownership.
- Related identifiers use one consistent vocabulary across backend, frontend, tests, and documentation.
- A shared type or mapper is not named for one incidental caller or stored under an operation that does not own it.
- C# casing guidance matches .NET conventions and no governing document gives contradictory rules.
- No rename silently changes an API route, JSON contract, persistence schema, financial behavior, authorization boundary, transaction behavior, ordering, or null semantics.
- Generated and externally constrained identifiers remain unchanged unless separately approved.
- Relevant builds, tests, architecture checks, repository naming scans, and `git diff --check` pass for every implementation stage.

### Decision record

- **Decision:** Pending system-wide assessment and owner approval of the staged rename set.
- **Owner rationale:** Naming must be coherent and unambiguous across classes and functions throughout the system; correcting only the reported case is insufficient.
- **Decision date:** Pending.
- **Implementation evidence:** Not started; `SimulationAccountListItem` / `ToListItem` is the first recorded assessment case.

## 4. Require server-side pagination and sorting for pageable resource collections

**Status:** Accepted; implementation not started.

### Current behavior

The shared success envelope already distinguishes ordinary list responses from paged responses. `ApiResults.Ok(...)` returns `{ data, meta }` without pagination, while `ApiResults.Page(...)` adds `pageNumber`, `pageSize`, `totalCount`, and `totalPages` under `meta.pagination`. Sorting metadata is not currently part of the response contract.

Current resource collections are inconsistent. For example, `SimulationAccountsGetList` accepts only `portfolioId`, returns an `IReadOnlyList<SimulationAccountListItem>`, and silently limits the Infrastructure query with `.Take(50)`. A caller cannot select a page or ordering and cannot determine whether additional rows exist. The value `50` is a defensive magic number rather than a documented Simulation Account business limit.

The architecture already permits two safe-read transports: `GET` for bounded scalar query parameters and HTTP `QUERY` for structured request content. The number of request properties alone does not determine the method; their structure and compatibility requirements do.

### Accepted decision

Every endpoint that retrieves a pageable collection of resource records must use server-side pagination and sorting. It must expose a `GetPage` or `SearchPage` operation, accept the shared page/sort contract, return a typed page result, and include both effective pagination and sorting metadata in every successful response.

Use `GET .../get-page` when identifiers, flat filters, page number, page size, and one admitted sort fit naturally in the URI query string. Use HTTP `QUERY .../search-page` with an explicit JSON `Content-Type` only when the search requires structured content such as nested filters, multiple ranges, large identifier sets, or multi-column sorting. Do not switch from `GET` to `QUERY` merely because a flat request has several properties.

This requirement applies to browsable resource collections backed by server-owned data. It does not force pagination onto `GetOne`, create/update/delete commands, finite combobox or autocomplete results, caller-bounded batch lookups, candles/time-series observations, or calculation/command results whose array is one atomic business result. Any new non-paged collection category requires an explicit naming-standard entry and architecture-test coverage rather than an incidental `.Take(...)` limit.

### Required contracts

The shared request model must carry at least:

```csharp
public sealed record PageRequest(
    int PageNumber,
    int PageSize,
    string SortBy,
    SortDirection SortDirection);
```

For `GET`, Minimal API endpoints bind the flat members from the query string through one composed parameter contract. For HTTP `QUERY`, the same logical page and sort values are carried in a structured JSON request contract. Transport binding types remain outside Domain and persistence projects.

The Application result must be typed rather than returning a bare collection:

```csharp
public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int PageNumber,
    int PageSize,
    string SortBy,
    SortDirection SortDirection);
```

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
- Apply filtering first, obtain `LongCountAsync`, apply the requested ordering, add a unique deterministic tie-breaker such as `Id`, and only then apply `Skip`/`Take` and projection.
- Validate page-offset arithmetic so an excessive page number cannot overflow the EF Core offset.
- Echo the normalized effective page and sorting values in response metadata so the client does not have to infer server defaults or normalization.
- Remove defensive collection magic numbers such as `.Take(50)` once the operation is migrated. A true business maximum must instead be named, documented, enforced on mutation, and tested independently from pagination.

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
- Add shared page, sort, page-result, pagination-metadata, and sorting-metadata contracts in their correct transport/Application ownership layers without introducing a mediator, runtime dispatcher, reflection scanning, or generic repository.
- Update `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md`, `docs/architecture/DOTNET_APPLICATION_ARCHITECTURE.md`, plan 0004, affected feature documentation, and `docs/project/PROJECT_STATE.md` only after behavior is implemented and verified.

### Required Angular changes for the initial migration

The `SimulationAccountsGetList` migration is not backend-complete until the Angular request, response, state, UI, and tests are migrated in the same implementation change. At minimum, update:

- `web/ledgerlens-web/src/app/api-routes.ts`: replace `SimulationAccountsGetList` with `SimulationAccountsGetPage` and require explicit `pageNumber`, `pageSize`, `sortBy`, and `sortDirection` query parameters with safe URI encoding.
- `web/ledgerlens-web/src/app/api-response.ts`: add sorting metadata and a typed way to retain both `meta.pagination` and `meta.sorting`; do not unwrap `data` and discard page metadata for paged operations.
- `web/ledgerlens-web/src/app/app.ts`: replace the bare-array load with a paged request, retain the effective page/sort state, handle empty and out-of-range pages, and reload deterministically after creating or selecting a Simulation Account.
- `web/ledgerlens-web/src/app/app.html`: add accessible paging controls and visible sort controls when more than one page or user-selectable sorting is available; preserve keyboard operation and clear disabled/current-page states.
- `web/ledgerlens-web/src/app/api-routes.spec.ts`: assert the canonical `get-page` route and every required encoded page/sort parameter.
- `web/ledgerlens-web/src/app/api-response.spec.ts` and `web/ledgerlens-web/src/app/app.spec.ts`: cover pagination/sorting metadata parsing, page navigation, sort changes, empty pages, API failures, and post-create reload behavior.

The Angular production build, focused component/contract tests, desktop layout, mobile layout, keyboard focus, and document-overflow checks are required acceptance evidence. A backend-only change or a client change that continues to assume a bare array does not satisfy this item.

### Acceptance criteria

- No pageable resource endpoint returns a silently truncated bare list.
- Every `GetPage` and `SearchPage` request supplies validated pagination and sorting, and every successful response includes matching `meta.pagination` and `meta.sorting`.
- `GET` remains the transport for flat page retrieval; HTTP `QUERY` is used only for structured search after the full compatibility gate passes.
- Sorting uses an explicit per-operation allowlist and a unique deterministic tie-breaker.
- Counts and page rows preserve the same filters and authorization boundary and execute server-side.
- Existing non-paged collection operations are retained only when they match a documented exception category.
- The Angular route builder, response contracts, state, paging/sort controls, and focused tests are migrated with the backend; page metadata is not discarded during response unwrapping.
- Backend, Angular, architecture, contract, integration, privacy, and `git diff --check` gates pass without changing financial calculations or leaking user data.

### Decision record

- **Decision:** Accepted.
- **Owner rationale:** Pageable resource collections must not rely on hidden hard-coded row limits. Pagination and sorting must be explicit, server-side, and present in both request and response contracts. `GET` remains appropriate for flat page parameters; HTTP `QUERY` is reserved for genuinely structured searches rather than selected solely because the property count grows.
- **Decision date:** 2026-09-22.
- **Implementation evidence:** Not started.

## 5. Restructure Angular by business feature and establish a global SCSS design system

**Status:** Accepted; implementation not started.

### Current behavior

The Angular application is a verified functional vertical shell, but its source is not yet organized for continued feature growth. `src/app/app.ts` currently owns most transport models, page state, request orchestration, formatting helpers, Portfolio workflows, Cash Ledger workflows, Market Data workflows, and Simulation workflows. `app.html` similarly renders the main real-portfolio and simulation experiences, while `app.routes.ts` contains no application routes. The extracted `api-routes.ts`, `api-response.ts`, and `simulation-chart.ts` files are useful first boundaries but do not yet form feature-owned frontend slices.

Global styling is currently split between `styles.css`, component CSS, and Tailwind utility usage. This does not guarantee that tables, selects, autocomplete controls, form fields, buttons, loading states, and error states share one visual contract or that changing one global definition updates every page. Page-specific customization also needs a supported override mechanism that does not depend on deep selectors, repeated hard-coded values, or uncontrolled `!important` rules.

### Accepted architecture

Restructure the Angular application as a feature-first, workflow-oriented standalone application. The root application becomes a thin shell containing global layout, navigation, notification/error presentation, and `router-outlet`. Business behavior is owned by lazy-loaded feature routes and their page, component, data-access, state, and model boundaries.

The initial top-level ownership is:

```text
src/app/
|-- app.ts
|-- app.html
|-- app.config.ts
|-- app.routes.ts
|-- core/
|   |-- api/
|   |-- configuration/
|   `-- layout/
|-- shared/
|   |-- formatting/
|   |-- models/
|   `-- ui/
`-- features/
    |-- portfolios/
    |-- investment-accounts/
    |-- cash-ledger/
    |-- simulation/
    `-- market-data/
```

Use `cash-ledger` rather than the ambiguous `ledger` feature name so it remains distinct from the LedgerLens product name and leaves room for stock-ledger, FX-cash-lot, and reconciliation capabilities. Frontend folders follow user workflows and cohesive UI ownership; they do not mirror every backend operation folder mechanically.

Each feature may contain only the layers it needs:

```text
features/<feature>/
|-- pages/
|-- components/
|-- data-access/
|-- models/
`-- <feature>.routes.ts
```

- Pages read route state, coordinate the user workflow, and connect feature state to presentational components.
- Components use explicit inputs and outputs and do not know API URLs.
- Data-access services own typed HTTP communication and success-envelope unwrapping.
- Feature stores use Angular signals and are provided at the narrowest appropriate route or feature boundary.
- Models remain feature-owned unless they are genuinely shared by multiple features.
- NgRx is not introduced without measured state complexity that cannot be handled clearly by route-scoped signal stores.
- Root or cross-feature services must not become a new all-purpose replacement for the current root component.

### Accepted global styling system

SCSS is the source of truth for application styling. Convert the global and component style entry points from CSS to SCSS and organize the global design system under `src/styles/`:

```text
src/
|-- styles.scss
`-- styles/
    |-- _tokens.scss
    |-- _themes.scss
    |-- _reset.scss
    |-- _typography.scss
    |-- _layout.scss
    |-- _forms.scss
    |-- _tables.scss
    |-- _buttons.scss
    |-- _overlays.scss
    `-- _utilities.scss
```

`styles.scss` is the single global entry point and composes the partials with Sass modules. SCSS organizes and generates the styles, while globally named CSS custom properties under `:root` provide runtime design tokens for colors, typography, spacing, control sizes, table density, borders, radii, shadows, focus indicators, and layering. Changing an admitted global token or shared component rule must update every consuming page without editing feature templates or feature styles.

Shared controls are implemented under `src/app/shared/ui/` and use an `ll-` selector/class prefix. At minimum, the admitted system owns consistent table, form-field, input, select/dropdown, autocomplete, button, loading, error, and empty-state primitives. Feature pages use these primitives rather than independently styling equivalent raw controls. Behavior, accessibility, and interaction remain component responsibilities; SCSS alone is not treated as an autocomplete or table abstraction.

Page-specific customization is supported through local component SCSS and inherited CSS custom-property overrides scoped beneath `:host` or an explicit page class. A feature may select an admitted variant such as compact density through a typed component input or host class. Page overrides must not mutate unrelated pages or depend on internal descendant structure that the shared component does not expose as part of its styling contract.

Tailwind utilities are migrated out of feature templates in stages. During migration, existing Tailwind behavior may remain only where not yet converted, but new or restructured UI uses the SCSS design system and shared UI primitives. Tailwind is removed from project configuration and dependencies after the final admitted template no longer requires it; the application must not retain two competing long-term sources of visual truth.

### Styling and encapsulation rules

- Do not use `::ng-deep`.
- Do not use `!important` except for a documented third-party integration that cannot be controlled through an admitted API, layer, or selector strategy.
- Do not hard-code a color, radius, shadow, spacing value, control height, or table density inside a feature when an admitted design token represents that concept.
- Keep local overrides scoped under `:host` or an explicit page/feature class.
- Style dropdown and autocomplete panels rendered outside a component host through the global overlay layer and theme tokens.
- Keep visible focus, keyboard operation, labels, error association, contrast, reduced-motion behavior, and responsive layout part of the shared component contract.
- Do not copy third-party component DOM selectors across pages. Wrap or configure a third-party control once behind the relevant shared UI boundary when one is admitted.
- A genuinely unique visualization or layout may keep feature-local SCSS, but it must consume global tokens for shared visual properties.

### Initial implementation sequence

1. Introduce `styles.scss`, design-token/theme partials, global foundations, and SCSS build configuration without changing business behavior.
2. Create the application shell and lazy standalone routes.
3. Extract Simulation first because it currently owns the largest distinct workflow, including its chart and Market Data coordination.
4. Extract Portfolios and Investment Accounts.
5. Extract Cash Ledger.
6. Extract reusable Market Data access and models without turning Market Data into a global catch-all service.
7. Introduce shared UI primitives as existing tables, form fields, dropdowns, autocomplete controls, buttons, and status views are migrated.
8. Remove migrated Tailwind utilities and remove Tailwind configuration/dependencies only after the final usage scan and production build pass.
9. Add enforceable architecture/static checks for forbidden direct HTTP ownership, feature-boundary violations, obsolete styling files, and disallowed styling escape hatches where checks can remain precise and maintainable.

Each stage must remain independently buildable and reviewable. Do not combine the restructure with financial calculation changes, backend contract redesign, or unrelated visual redesign. Existing behavior, request semantics, API response/error handling, numeric presentation, accessibility, and responsive behavior are preserved unless a separately accepted item explicitly changes them.

### Implementation scope

- `web/ledgerlens-web/src/app/` application shell, routes, feature folders, shared UI, core API/configuration/layout, models, and tests
- `web/ledgerlens-web/src/styles.scss` and `web/ledgerlens-web/src/styles/` SCSS design-system sources
- Component `.css` files migrated to `.scss` where retained
- `web/ledgerlens-web/angular.json`, package scripts, and dependencies required by the SCSS/Tailwind migration
- Focused Angular architecture/static tests for the accepted ownership and styling rules
- Relevant architecture, frontend, testing, and project-state documentation as implementation stages become verified behavior

### Acceptance criteria

- `app.ts` is a thin application shell and does not own Portfolio, Investment Account, Cash Ledger, Simulation, or Market Data workflows.
- Business pages are reachable through lazy-loaded standalone feature routes with a deterministic default route and fallback behavior.
- No feature page or presentational component constructs API URLs; typed feature data-access boundaries own HTTP communication.
- Feature state uses signals with explicit loading, failure, empty, and selected-item states and is scoped no wider than required.
- Tables, selects/dropdowns, autocomplete controls, form fields, buttons, loading states, error states, and empty states use admitted shared UI primitives and the global design tokens.
- Changing an admitted global table or form-control token demonstrably updates every consuming feature without modifying those feature templates or local SCSS files.
- A page can override an admitted token or variant under `:host` or its page class without changing another page.
- Overlay content uses the same active tokens and theme as its invoking page and remains keyboard and screen-reader operable.
- No `::ng-deep` is introduced, and every exceptional `!important` use has a documented third-party justification and focused regression coverage.
- Feature SCSS does not duplicate admitted global colors, spacing, radii, control dimensions, table density, or focus styles with hard-coded values.
- Tailwind is not used by new or restructured feature UI and is fully removed once no admitted template depends on it.
- Existing API envelope/Problem Details behavior, financial calculations, numeric formatting, correction semantics, privacy boundaries, and loopback-only posture remain unchanged.
- Focused component and contract tests, Angular tests, production build, dependency/privacy checks, desktop and mobile visual QA, keyboard/focus checks, document-overflow checks, and `git diff --check` pass for the completed implementation stages.

### Decision record

- **Decision:** Accepted.
- **Owner rationale:** LedgerLens must scale by cohesive business feature while keeping one globally controlled visual language. Tables, dropdowns, autocomplete controls, and other shared UI must change consistently from the global SCSS design system, while individual pages retain a safe, scoped override mechanism.
- **Decision date:** 2026-09-22.
- **Implementation evidence:** Not started.
