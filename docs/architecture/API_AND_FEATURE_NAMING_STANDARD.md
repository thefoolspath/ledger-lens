# API and Feature Naming Standard

Status: Accepted and implemented for all current Version 1 business endpoints; runtime re-verification completed in [plan 0003](../plans/completed/0003-api-feature-naming-migration.md). The unified response contract is in implementation under [plan 0004](../plans/active/0004-unified-api-response-contract.md). Last reviewed: 2026-09-21.

## Purpose

This document is the normative LedgerLens terms-of-reference for naming business HTTP routes and their vertical-slice implementation. Its goal is deterministic traceability: a developer can derive one `OperationKey` from a public route and use that key to find the endpoint, request, response, application handler, persistence operation, Angular client, and tests for that operation.

`MUST` and `MUST NOT` are mandatory. `SHOULD` describes the default and requires a documented reason to deviate. `MAY` is optional. The implemented Version 1 business routes conform to this standard; plan 0003 records the migration and verification evidence.

## Canonical route grammar

Public business routes MUST use:

```text
/api/{service}/v1/{plural-feature-path}/{operation}[/{resourceId}]
```

The owning backend receives the same path after Gateway removes `/api/{service}`:

```text
/v1/{plural-feature-path}/{operation}[/{resourceId}]
```

Rules:

- Static path segments MUST use lowercase `kebab-case`; uppercase characters and underscores are prohibited.
- Business resource and feature segments MUST be plural nouns, such as `users`, `portfolios`, `investment-accounts`, and `simulation-accounts`.
- The operation MUST be the final static segment. A resource identifier MAY follow it.
- Route and query parameter names MUST use `camelCase`, for example `{portfolioId:guid}` and `?portfolioId=...`.
- A canonical route MUST NOT end with a trailing slash.
- Gateway service prefixes and API versions are transport concerns and are not part of the `OperationKey`.
- `/health`, `/alive`, and Development-only `/internal/ping` are reserved platform endpoints and are exempt from the business-route grammar.

## OperationKey derivation

Derive the `OperationKey` by removing `/api/{service}/v1/`, omitting route parameters and the query string, and converting every remaining static segment to PascalCase without changing its order.

| Public route | OperationKey |
| --- | --- |
| `/api/portfolio/v1/portfolios/create` | `PortfoliosCreate` |
| `/api/portfolio/v1/portfolios/get-one/{portfolioId}` | `PortfoliosGetOne` |
| `/api/portfolio/v1/portfolios/get-list` | `PortfoliosGetList` |
| `/api/portfolio/v1/simulation-accounts/get-page` | `SimulationAccountsGetPage` |
| `/api/portfolio/v1/users/roles/get-combobox-list` | `UsersRolesGetComboboxList` |

Every operation-specific production type and test MUST begin with its `OperationKey`. Every ASP.NET Core endpoint MUST use the same value as its endpoint name, for example `.WithName("PortfoliosCreate")`. The value is also the preferred OpenAPI operation identifier and structured-log operation name when those surfaces are enabled.

## Approved operation vocabulary

### Queries and result cardinality

| Route operation | Meaning | Required successful JSON shape |
| --- | --- | --- |
| `get-one` | Retrieve one identified logical record | One JSON object; absence is `404` |
| `get-list` | Retrieve a bounded, non-paged collection | A top-level JSON array; no matches is `[]` |
| `get-page` | Retrieve a paged collection | `{ items, totalCount, pageNumber, pageSize }` |
| `search-list` | Filter/search and return a bounded collection | A top-level JSON array |
| `search-page` | Filter/search and return a page | The standard page object |
| `get-latest-list` | Retrieve the latest available observation for each bounded requested item | A top-level JSON array |
| `get-autocomplete-list` | Server-filtered suggestions for typed input | A bounded top-level JSON array |
| `get-combobox-list` | A finite option set for a combobox | A bounded top-level JSON array |
| `export-file` | Export a downloadable representation | A file response, not JSON collection data |

`List` describes zero-to-many logical records even when an item is a projection rather than a domain entity. `Page` MUST be used whenever pagination metadata is returned. `One` MUST NOT return an array, and `List` MUST NOT silently switch to a page envelope.

The operation categories in this table remain normative. Current Portfolio Core and Market Data business JSON responses use the plan-0004 shared `{ data, meta }` envelope: object results remain objects inside `data`, list results remain arrays inside `data`, and page metadata belongs in `meta.pagination`.

Autocomplete endpoints MUST accept a bounded search term and enforce a result limit. Combobox endpoints MUST represent a finite option set. A lookup reused by multiple screens SHOULD live under the resource that owns the data, such as `roles/get-combobox-list`; a screen-specific lookup MAY use a nested feature path such as `users/roles/get-combobox-list`.

### Commands

Approved command operations are:

```text
create, update, patch, delete, restore, activate, deactivate,
confirm, cancel, correct, reverse, create-bulk, update-bulk
```

Use a more specific domain verb only when it expresses an established business transition and is entered in the glossary below. `create-bulk` and `update-bulk` MUST identify partial-success or atomic behavior explicitly in their contract; a generic `save` endpoint is prohibited.

The Version 1 migration admits two simulation-specific operations:

| Route operation | Meaning | Successful JSON shape |
| --- | --- | --- |
| `record-list` | Calculate multiple current valuations and persist complete immutable snapshots | A top-level JSON array |
| `calculate-series-list` | Calculate multiple historical series points without persistence | A top-level JSON array |

No other `{verb}-list` command is admitted implicitly. Additions require the exception/review process in this standard and a glossary update.

### Prohibited operation names

The following names are ambiguous and MUST NOT be introduced: `get`, `get-data`, `list`, `list-list`, `save`, `save-data`, `process`, `manage`, `popup`, `lookup`, and `do-action`. Variants that differ only by casing or punctuation are also prohibited.

## HTTP semantics and status codes

- `GET` MUST be used for safe reads whose bounded scalar filters, sorting, and pagination fit in the URI.
- HTTP `QUERY` MAY be used for safe structured reads only after the compatibility gate in [.NET Application Architecture](DOTNET_APPLICATION_ARCHITECTURE.md#http-get-and-query-policy) passes.
- A documented `POST .../search-list` or `POST .../search-page` MAY provide compatibility for a structured safe read when an intermediary cannot carry `QUERY`; it MUST NOT be described as cache-equivalent to `GET`.
- `POST` MUST be used for `create` and business commands such as `confirm`, `correct`, and `cancel`.
- `PUT` MUST represent a complete `update`; `PATCH` MUST represent a partial update.
- `DELETE` MUST represent `delete` when the contract uses HTTP deletion semantics.

Successful creation returns `201 Created`. Successful reads return `200 OK`. A missing `get-one` resource returns `404 Not Found`. Updates return `200 OK` when they have a response body or `204 No Content` otherwise. Successful deletion returns `204 No Content`. Validation failures use RFC Problem Details. Conflicting state transitions return `409 Conflict`; an idempotent command MAY return the previously committed result when that behavior is part of its contract.

## Unified response and error standard

The following contract is implemented for current Portfolio Core and Market Data endpoints; plan 0004 remains active until its complete error, localization, distributed-runtime, and closeout gates pass:

- Successful business JSON responses with bodies use `ApiResponse<T>` containing required `data` and `meta`. Metadata always carries `correlationId`; paged operations additionally carry `pageNumber`, `pageSize`, `totalCount`, and `totalPages` under `meta.pagination`.
- A missing single resource remains `404`, empty collections remain arrays, `201 Created` retains `Location`, and `204`, file/stream, health, liveness, and Development ping responses are not wrapped.
- Failures use RFC Problem Details rather than the success envelope. They add stable non-localized `code`, W3C `traceId`, `correlationId`, and validation `errors` when applicable. HTTP status remains authoritative and must match the body.
- User-facing error text is localized through English and Thai RESX resources selected by `Accept-Language`, with English fallback. Expected failures carry stable codes and safe arguments; raw exception messages, stack traces, secrets, provider payloads, and personal data never enter responses.
- Ordinary validation and domain error messages are not database content. Any future administrator-editable catalog requires a separate ADR and explicit availability, cache, authorization, audit, fallback, and safe-formatting design.

Plan 0004 introduces a transport-only `LedgerLens.ApiContracts` project and keeps ASP.NET Core response/localization mechanics in `LedgerLens.ServiceDefaults`. Domain and Application projects MUST NOT depend on the HTTP contracts; expected Application failures remain typed and transport-independent.

## Folder, file, type, and payload naming

A vertical slice SHOULD mirror the feature and operation in every participating project:

```text
Api/Features/Users/Create/
  UsersCreateEndpoint.cs
  UsersCreateRequest.cs
  UsersCreateResponse.cs

Application/Features/Users/Create/
  UsersCreateCommand.cs
  UsersCreateHandler.cs
  IUsersCreateStore.cs

Infrastructure/Features/Users/Create/
  UsersCreateStore.cs

Tests/Features/Users/Create/
  UsersCreateTests.cs
```

Rules:

- A production C# file MUST contain at most one top-level public class, record, struct, enum, or interface, and its filename MUST equal that type name.
- Operation-specific C# types MUST use `PascalCase` and begin with the exact `OperationKey`.
- C# methods, parameters, and local variables MUST use `camelCase`. Standard .NET acronym casing applies: `Id`, `Uri`, `Usd`, and `Thb`.
- Angular filenames MUST use `kebab-case`, for example `users-create.api.ts`. Exported Angular/TypeScript types MUST use the `OperationKey`, and client methods MUST use its camelCase form, such as `usersCreate()`.
- JSON properties MUST use `camelCase`. PostgreSQL tables and columns retain the accepted `snake_case` persistence convention.
- A shared domain entity, value object, calculation policy, generated persistence type, or cross-operation infrastructure helper MUST keep its domain name and MUST NOT be duplicated merely to include an `OperationKey`.
- A private nested helper or DTO used by only one owning type MAY remain in that type's file. A reused type MUST move to its own accurately named file.
- A slice MUST keep endpoints thin, application dependencies explicit, and business policy in Domain/Application. This standard does not authorize mediator packages, generic repositories, service location, domain entities as transport contracts, or business logic in endpoints.

## Glossary

Approved static terms are canonical and MUST be reused rather than replaced with synonyms:

| Concept | Route term | C# term |
| --- | --- | --- |
| User | `users` | `Users` |
| Portfolio | `portfolios` | `Portfolios` |
| Investment account | `investment-accounts` | `InvestmentAccounts` |
| Cash ledger entry | `cash-ledger-entries` | `CashLedgerEntries` |
| Simulation account | `simulation-accounts` | `SimulationAccounts` |
| Simulation trade draft | `simulation-trade-drafts` | `SimulationTradeDrafts` |
| Simulation trade | `simulation-trades` | `SimulationTrades` |
| Simulation valuation | `simulation-valuations` | `SimulationValuations` |
| Instrument | `instruments` | `Instruments` |
| Instrument candle series | `instrument-candles` | `InstrumentCandles` |
| Quote | `quotes` | `Quotes` |
| Foreign-exchange rate | `foreign-exchange-rates` | `ForeignExchangeRates` |

Adding or changing a glossary term is an architecture change and requires review of routes, contracts, Angular usage, tests, and documentation before acceptance.

## Version 1 migration map

The following replacements were implemented atomically in plan 0003. The former routes have no compatibility aliases.

### Portfolio Core

| Current backend route | Planned backend route | Planned OperationKey |
| --- | --- | --- |
| `GET /v1/portfolios` | `GET /v1/portfolios/get-list` | `PortfoliosGetList` |
| `GET /v1/portfolios/{portfolioId}` | `GET /v1/portfolios/get-one/{portfolioId}` | `PortfoliosGetOne` |
| `POST /v1/portfolios` | `POST /v1/portfolios/create` | `PortfoliosCreate` |
| `POST /v1/portfolios/{portfolioId}/accounts` | `POST /v1/investment-accounts/create` | `InvestmentAccountsCreate` |
| `POST /v1/accounts/{accountId}/ledger-entries` | `POST /v1/cash-ledger-entries/create` | `CashLedgerEntriesCreate` |
| `POST /v1/ledger-entries/{entryId}/corrections` | `POST /v1/cash-ledger-entries/correct/{entryId}` | `CashLedgerEntriesCorrect` |
| `GET /v1/portfolios/{portfolioId}/simulation-accounts` | `GET /v1/simulation-accounts/get-list?portfolioId=...` | `SimulationAccountsGetList` |
| `POST /v1/portfolios/{portfolioId}/simulation-accounts` | `POST /v1/simulation-accounts/create` | `SimulationAccountsCreate` |
| `GET /v1/simulation-accounts/{accountId}` | `GET /v1/simulation-accounts/get-one/{accountId}` | `SimulationAccountsGetOne` |
| `POST /v1/simulation-accounts/{accountId}/trade-drafts` | `POST /v1/simulation-trade-drafts/create` | `SimulationTradeDraftsCreate` |
| `POST /v1/simulation-accounts/{accountId}/trade-drafts/{draftId}/confirm` | `POST /v1/simulation-trade-drafts/confirm/{draftId}` | `SimulationTradeDraftsConfirm` |
| `POST /v1/simulation-trades/{tradeId}/corrections` | `POST /v1/simulation-trades/correct/{tradeId}` | `SimulationTradesCorrect` |
| `POST /v1/simulation-accounts/{accountId}/valuations` | `POST /v1/simulation-valuations/record-list` | `SimulationValuationsRecordList` |
| `POST /v1/simulation-accounts/{accountId}/valuation-series` | `POST /v1/simulation-valuations/calculate-series-list` | `SimulationValuationsCalculateSeriesList` |

`record-list` and `calculate-series-list` are admitted domain-specific commands for the existing simulation contracts: both return multiple logical results, while only `record-list` persists immutable snapshots. Account/portfolio ownership identifiers removed from the route hierarchy MUST remain explicit request inputs and MUST retain their existing server-side ownership validation.

### Market Data

| Current backend route | Planned backend route | Planned OperationKey |
| --- | --- | --- |
| `GET /v1/instruments/search` | `GET /v1/instruments/search-list` | `InstrumentsSearchList` |
| `POST /v1/quotes/latest` | `POST /v1/quotes/get-latest-list` | `QuotesGetLatestList` |
| `GET /v1/instruments/{instrumentId}/candles` | `GET /v1/instrument-candles/get-one/{instrumentId}` | `InstrumentCandlesGetOne` |
| `POST /v1/fx/latest` | `POST /v1/foreign-exchange-rates/get-latest-list` | `ForeignExchangeRatesGetLatestList` |

The existing `POST` latest-quote and FX reads remain compatibility reads unless the HTTP `QUERY` gate is completed. Their provider, provenance, freshness, quota, and retention semantics MUST not change during naming migration.

## Examples

Conforming:

```text
GET  /api/portfolio/v1/users/get-one/{userId}
GET  /api/portfolio/v1/users/get-list
GET  /api/portfolio/v1/users/get-page?pageNumber=1&pageSize=50
GET  /api/portfolio/v1/users/get-autocomplete-list?query=ade&limit=10
GET  /api/portfolio/v1/users/roles/get-combobox-list
POST /api/portfolio/v1/users/create
PATCH /api/portfolio/v1/users/patch/{userId}
```

Nonconforming:

```text
/api/portfolio/v1/User/Create          # uppercase path segment
/api/portfolio/v1/user/create          # singular business resource
/api/portfolio/v1/users/GetData        # casing and ambiguous operation
/api/portfolio/v1/users/list-list      # duplicated/ambiguous operation
/api/portfolio/v1/users/save-data      # hides create/update semantics
/api/portfolio/v1/users/popup          # UI widget is not a data contract
```

## Route-to-code search procedure

For `/api/portfolio/v1/simulation-accounts/get-list?portfolioId=...`:

1. Remove `/api/portfolio/v1/` and the query string.
2. Convert `simulation-accounts/get-list` to `SimulationAccountsGetList`.
3. Search the repository for `SimulationAccountsGetList`.
4. Confirm that the results include the endpoint, request/query contract when present, response/result, handler, persistence operation, Angular call, and tests.
5. Follow referenced shared Domain types normally; do not duplicate them to force the key into every shared filename.

## Enforcement and exceptions

Plan 0003 added architecture and contract tests for route casing, plural features, approved operations, unique endpoint names, file/type alignment, one public top-level type per production file, and query response cardinality. Angular and repository-owned tests now use the canonical routes, and the old business routes no longer occur in production code or tests.

An exception requires all of the following:

1. A concrete interoperability, standards, or compatibility reason.
2. The exact route/type/file scope and an owner.
3. An architecture-test allowlist entry that cannot match unrelated operations.
4. A removal or review condition.
5. An ADR when the exception changes public contract semantics, dependency direction, or a costly-to-reverse convention.

Convenience, personal casing preference, or copying a legacy route is not sufficient justification.
