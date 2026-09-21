# Unified API Response Contract

Last reviewed: 2026-09-21. Status: In progress.

## Goal

Adopt one predictable HTTP response family across LedgerLens business APIs without weakening HTTP semantics: successful JSON responses use a shared `ApiResponse<T>` envelope, while failures use RFC Problem Details extended with stable error codes and request diagnostics. Add English and Thai user-facing error resources without storing ordinary validation or domain error text in the database.

This plan follows the completed [API and Feature Naming Standard Migration](../completed/0003-api-feature-naming-migration.md). The shared contract foundation, common localized Problem Details behavior, Portfolio Core and Market Data success envelopes, Angular unwrapping, and host registration are now implemented. Full typed expected-failure coverage, service-specific message catalogs, and the complete closeout gate remain before this plan can move to `completed/`.

## Boundaries and decisions

- Create a lightweight `LedgerLens.ApiContracts` project for transport-only response records and common machine-readable error codes. Do not place these contracts in `LedgerLens.ServiceDefaults`, `LedgerLens.IntegrationContracts`, a Domain project, or an Application project.
- Keep ASP.NET Core registration, correlation access, response factories, Problem Details customization, and request-localization setup in `LedgerLens.ServiceDefaults`; API hosts opt in explicitly.
- Apply the contract to business JSON endpoints in Portfolio Core, Market Data, and future Slip Import, Research, and Operations APIs. Do not wrap `/health`, `/alive`, Development-only `/internal/ping`, file/stream responses, or `204 No Content`.
- Preserve the canonical Version 1 routes, OperationKeys, business calculations, ownership checks, idempotency, provider evidence, transaction boundaries, status codes, and database schema established by plan 0003.
- Do not add MediatR, mapper frameworks, generic repositories, a runtime service locator, an error-message database table, or an EF migration.
- Before implementation, confirm that no external Version 1 consumer depends on the current response bodies. If one exists, stop and replace the in-place migration with a versioned compatibility plan.

## Public success contract

Every successful business JSON response with a body uses these camel-case shapes:

```json
{
  "data": {},
  "meta": {
    "correlationId": "01f..."
  }
}
```

```json
{
  "data": [],
  "meta": {
    "correlationId": "01f...",
    "pagination": {
      "pageNumber": 1,
      "pageSize": 50,
      "totalCount": 125,
      "totalPages": 3
    }
  }
}
```

The shared records are:

- `ApiResponse<T>` with required `Data` and `Meta`.
- `ApiResponseMeta` with required `CorrelationId` and optional `Pagination` omitted from JSON when absent.
- `PaginationMetadata` with `PageNumber`, `PageSize`, `TotalCount`, and `TotalPages`; counts use a 64-bit integer where the underlying query count can exceed `int`.

Rules:

- `GetOne` returns `ApiResponse<T>`; absence remains `404`, never `200` with `data: null`.
- `GetList`, `SearchList`, autocomplete, combobox, latest-list, and admitted list-returning commands return `ApiResponse<IReadOnlyList<T>>`; no matches is `data: []`.
- `GetPage` and `SearchPage` return the item array in `data` and offset pagination in `meta.pagination`.
- `201 Created` wraps its representation and retains a correct `Location` header. A successful operation intentionally returning no representation uses `204 No Content` without an envelope.
- Do not add `IsError`, `StatusCode`, `Desc`, or `ErrorMessage` to the success envelope. HTTP status is authoritative, and failure details belong to Problem Details.
- Version 1 supports page-number pagination only. Cursor pagination requires a separately named contract when a demonstrated use case exists.

## Public failure contract and message policy

Failures use `application/problem+json` and the standard Problem Details members `type`, `title`, `status`, `detail`, and `instance`, extended with:

- `code`: stable, non-localized, lowercase dot-separated machine code such as `common.validation.failed`, `portfolio.simulation_trade.invalid_state`, or `market.provider.unavailable`.
- `traceId`: current W3C activity trace identifier.
- `correlationId`: the same value emitted in `X-Correlation-ID` and the structured logging scope.
- `errors`: field-keyed arrays only for validation failures.

Expected Application failures become typed operation results with a stable failure kind and safe formatting arguments; Application and Domain remain independent of HTTP and localization. API endpoints map those results to status, code, and localized Problem Details. Central exception handling maps unexpected exceptions to a generic `500` response and logs diagnostic details without returning raw exception messages, stack traces, secrets, provider payloads, or personal data.

Use the following status families consistently: validation/binding/invalid UUID is `400`, unauthenticated is `401`, forbidden is `403`, missing resource is `404`, state conflict is `409`, unsupported media type is `415`, unexpected failure is `500`, and unavailable dependency/provider is `503`. The HTTP status and Problem Details `status` member must match; an error must never be returned with a `2xx` status.

Stable error codes remain in source control. Generic user-facing titles/details live in shared RESX resources, while service-specific titles/details live in the owning API project. Provide `en-US` and `th-TH`, select with `Accept-Language`, and default or fall back to English. A missing localized key falls back to English; if the English key is also missing, emit a generic safe message and a structured missing-resource log. Resource formatting accepts only explicitly allowlisted values, never raw exception text.

Do not store ordinary error messages in PostgreSQL. A future administrator-editable business-content requirement needs a separate ADR covering ownership, authorization, cache invalidation, fallback behavior, availability, audit history, and safe formatting before a database-backed catalog is admitted.

## Milestone 1 — shared contract foundation

Add `LedgerLens.ApiContracts`, its response records, common error-code vocabulary, and focused serialization tests. Add ServiceDefaults helpers that create typed success results from `HttpContext`, attach the established correlation value, configure Problem Details extensions, and register supported request cultures without changing platform endpoints.

**Acceptance:** exact JSON fixtures prove ordinary, empty-list, paged, created, no-content, file, and platform response behavior; architecture tests keep shared transport contracts out of Domain/Application and keep service-specific contracts in their owning service.

## Milestone 2 — error and localization foundation

Define the common and service-specific RESX catalogs in English and Thai. Replace client-visible raw exception messages and ad hoc `{ error = ... }` bodies with typed expected failures and centralized unexpected-exception handling. Preserve internal diagnostic messages for logs while preventing them from crossing the HTTP boundary.

**Acceptance:** tests cover every admitted status family, validation `errors`, English/Thai selection, unsupported-language fallback, missing-resource fallback, correlation/trace identifiers, content type, and non-disclosure of exception text or credentials.

## Milestone 3 — Portfolio Core and Market Data migration

Migrate all current business endpoints and their operation-specific contract tests in one coordinated change. Keep the plan 0003 routes and OperationKeys unchanged. Update Angular operation clients to model `ApiResponse<T>`, unwrap `data` through one shared helper, consume pagination metadata explicitly, and parse typed Problem Details for user-visible failures.

**Acceptance:** direct-backend and Gateway responses agree; Angular handles successful object/list responses and localized validation/provider failures; financial results, Market Data provenance, Created locations, idempotency, correction semantics, and ownership behavior are unchanged.

## Milestone 4 — remaining hosts and enforcement

Register the common failure behavior in Slip Import, Research, and Operations API hosts without inventing placeholder business endpoints. Keep Gateway-generated failures standards-compatible and ensure proxied backend bodies are not double-wrapped. Add architecture/contract rules that reject new ad hoc business success envelopes, anonymous error objects, raw exception details, and success bodies that bypass the shared contract without an approved exception.

**Acceptance:** future service endpoints have one documented path to compliant results; health and infrastructure endpoints remain exempt; Gateway timeout/unavailable behavior has explicit Problem Details coverage.

## Milestone 5 — verification and closeout

Run the complete .NET and Angular builds, unit tests, architecture tests, Gateway integration tests, component tests, explicitly enabled distributed scenario, EF model-drift check, dependency audit, privacy/credential scans, contract-shape scans, and `git diff --check`. Verify both languages through direct backend and Gateway requests and inspect the final diff for accidental route, schema, calculation, or persistence changes.

**Acceptance:** every current business JSON endpoint follows the success/failure contract or has a narrowly documented exception; no EF migration or route compatibility alias exists; all available gates pass; documentation records exact evidence before the plan moves to `completed/`.

## Implementation evidence — 2026-09-21

- **PASS:** the project-local .NET solution build completed with zero errors using a single MSBuild worker; three existing EF Core 10.0.0/10.0.8 assembly-version warnings remain visible.
- **PASS:** Architecture tests `17/17`, Gateway integration tests `3/3`, Portfolio Core unit tests `10/10`, Market Data unit test `1/1`, Angular tests `8/8`, Angular production build, and `git diff --check` passed.
- **IMPLEMENTED:** `LedgerLens.ApiContracts`, shared success helpers, English/Thai common RESX resources, Problem Details diagnostics, Portfolio Core and Market Data endpoint migration, Angular response unwrapping and problem parsing, and response registration in all current API hosts and Gateway.
- **NOT CLOSED:** the explicitly enabled Aspire/PostgreSQL/NATS scenario, EF model-drift check, dependency/privacy scans, complete status/localization/non-disclosure matrix, and service-specific typed expected-failure catalogs were not re-run or completed in this change.

## Completion criteria

- Successful business JSON bodies use `ApiResponse<T>` and never encode an error state.
- Failure bodies use localized RFC Problem Details with stable code, trace ID, correlation ID, and validation errors where applicable.
- Page responses place items in `data` and pagination in `meta.pagination`; list responses preserve an empty array.
- English and Thai resources have tested fallback behavior, and no raw exception message is exposed.
- Angular, Gateway, backend APIs, distributed scenarios, and contract fixtures agree on the new shapes.
- Routes, OperationKeys, financial invariants, database schema, ownership, idempotency, and provider evidence remain unchanged.
- The API standard, testing documentation, project state, and this plan agree on the verified implementation status.

## Documentation rule

Each implementation milestone updates this plan and at least one relevant architecture, quality, or project-state document in the same task. Change `Status: Planned` only when implementation begins. Until completion, documentation must distinguish the accepted future contract from the currently implemented raw response shapes.
