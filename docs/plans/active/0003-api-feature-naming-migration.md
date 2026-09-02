# API and Feature Naming Standard Migration

Last reviewed: 2026-09-02. Status: Implemented; distributed runtime verification blocked by local container connectivity.

## Goal

Adopt the accepted [API and Feature Naming Standard](../../architecture/API_AND_FEATURE_NAMING_STANDARD.md) across every LedgerLens business service so one deterministic `OperationKey` traces a public route through backend, persistence, Angular, and tests. Migrate the existing Version 1 contracts atomically without changing business behavior, financial invariants, ownership, transaction boundaries, provider evidence, or database schema.

## Boundaries

- Apply the standard to Portfolio Core, Market Data, future Slip Import, Research, and Operations business endpoints plus their Gateway/Angular consumers and tests.
- Keep `/health`, `/alive`, `/internal/ping`, Gateway service prefixes, and infrastructure resource names unchanged.
- Replace existing Version 1 business routes in one coordinated repository change; do not retain compatibility aliases.
- Before implementation, confirm that no external consumer exists outside this repository. If one exists, stop and replace the in-place migration with an explicitly versioned compatibility plan.
- Do not add MediatR, a runtime dispatcher, generic repositories, mapper frameworks, validation frameworks, or extra unit-of-work abstractions.
- Do not create an EF migration; this plan changes naming, contracts, and source organization only.

## Milestone 1 — naming foundation

Publish the normative standard, freeze the Version 1 migration map, and add architecture tests that fail for nonconforming business routes, duplicate endpoint names, invalid operation vocabulary, filename/public-type mismatch, or multiple top-level public production types without a narrow allowlist.

Add contract-test fixtures proving the three collection shapes: `GetOne` is one object or `404`, `GetList` is a top-level array with `[]` for no matches, and `GetPage` is the standard `{ items, totalCount, pageNumber, pageSize }` object. Document the approved glossary and make exceptions evidence-based, scoped, and reviewable.

**Acceptance:** the rules identify current violations without misclassifying platform endpoints, generated EF files, migrations, or private nested helpers; every planned route has one unique OperationKey; tests prove both conforming and rejected examples.

## Milestone 2 — Portfolio Core migration

Split the current combined endpoint, command, handler, request, response, abstraction, and persistence files into feature/operation slices named by the migration map. Keep one top-level public type per production file. Use explicit handlers and operation-specific persistence ports or focused persistence files where that improves traceability; shared Domain types, generated persistence models, and cohesive mapping helpers remain shared.

Replace all Portfolio Core Version 1 business routes with the canonical routes and endpoint names. Move portfolio/account identifiers from legacy parent route segments into explicit request inputs where the map requires it. Preserve UUIDv7 validation, fixed-current-user ownership filtering, ledger reversal/replacement semantics, idempotent draft confirmation, serializable transactions, retry execution strategies, FIFO calculations, decimal scales, bounded queries, and cancellation.

**Acceptance:** searching each Portfolio Core OperationKey finds its endpoint, transport contract, handler, persistence operation, Angular usage, and test; existing observable success/error behavior and financial results remain covered; no legacy Portfolio Core business route remains.

## Milestone 3 — Market Data migration

Split Market Data endpoints and handlers into operation-keyed slices. Rename instrument search, latest quote, candle series, and foreign-exchange routes according to the standard while retaining the current compatible HTTP methods until the separate HTTP `QUERY` gate passes.

Preserve provider selection, deterministic synthetic data, symbol/market validation, provenance, feed, freshness, delay, request identity, quota behavior, retention-policy keys, cancellation, Problem Details, and credential redaction.

**Acceptance:** the four Market Data OperationKeys trace through API, Application, Infrastructure, Angular, and tests; response cardinality matches the route name; no old Market Data route remains; external-provider behavior is not newly enabled.

## Milestone 4 — remaining services

Create the feature-folder convention and naming-test coverage for future Slip Import, Research, and Operations business slices. Do not create placeholder business endpoints merely to satisfy folder structure. Keep their current foundation ping and health surfaces exempt.

**Acceptance:** new business endpoints in these services cannot compile/pass architecture checks unless they follow the standard; no speculative domain code or database object is introduced.

## Milestone 5 — client and contract migration

Update Angular route constants/calls, TypeScript request/response types, methods, component tests, Gateway integration tests, distributed synthetic scenarios, and Created-location assertions in the same change as backend routes. Use operation-keyed Angular types and camelCase client methods. Retain the mutation marker, loopback-origin protection, Gateway-only browser access, accessible Simulation UI, and existing error/incomplete states.

Remove every repository-owned reference to the replaced business routes. Do not leave deprecated aliases, fallback retries, or dual-path client behavior.

**Acceptance:** Angular calls only canonical Gateway routes; Gateway path removal still reaches the matching backend route; direct backend and through-Gateway contracts agree; no old Version 1 business route exists in production code or tests.

## Milestone 6 — verification and closeout

Run the complete .NET build and relevant unit, architecture, Gateway, component, integration, and explicitly enabled distributed tests. Run the Angular tests and production build, Portfolio Core EF model-drift check, production dependency audit, tracked privacy/credential scans, route-string scan, and `git diff --check`. Verify searchability for every OperationKey and inspect the final diff for accidental business or schema changes.

Docker-unavailable tests remain an external blocker rather than a naming exception. Do not mark the plan complete until required PostgreSQL scenarios have current execution evidence or the project state explicitly records the same external block.

**Acceptance:** all available gates pass; no EF migration or model drift exists; no business calculation, ownership rule, transaction behavior, provider contract, or response cardinality changed unintentionally; documentation records exact commands and results.

## Completion criteria

- Every implemented business endpoint conforms to the standard or has a narrowly documented, tested exception.
- No non-generated production file contains multiple top-level public types without an approved allowlist entry.
- `GetOne`, `GetList`, and `GetPage` use their required JSON shapes.
- Angular uses only canonical routes, names, and contracts.
- Replaced Version 1 business routes no longer occur in production code or repository-owned tests.
- Business behavior, exact financial calculations, user ownership, correction/idempotency rules, and transaction boundaries are unchanged.
- No EF migration is created and the Portfolio Core model-drift check remains clean.
- `PROJECT_STATE.md`, this plan, architecture documentation, and verification evidence agree on the implemented status.

## Implementation and verification evidence — 2026-09-02

- The external-consumer gate passed: the repository has no release, tag, deployment configuration, package consumer, or owner-authorized publication, and the documented release policy prohibits publication without owner approval.
- All 18 implemented Portfolio Core and Market Data business routes were replaced by the frozen migration map without compatibility aliases. Parent portfolio/account identifiers removed from route hierarchy are explicit request or query inputs; UUIDv7 and ownership checks remain at the same boundaries.
- Operation-keyed request, command, handler, persistence method, endpoint name, Angular route constant, and test references are searchable for every operation. Non-generated production C# types were split to one public top-level type per matching file; generated Database types, migrations, and top-level `Program.cs` remain exempt.
- `ApiNamingConventionTests` enforce the complete current route set, canonical casing/plural/operation vocabulary, deterministic and unique endpoint names, conforming/rejected examples, file/type alignment, cross-layer OperationKey traceability, and `GetOne`/`GetList`/`GetPage` JSON fixtures. Architecture tests pass `16/16`; Portfolio Core unit tests pass `10/10`; Market Data unit tests pass `1/1`; Gateway integration tests pass `2/2`; guarded distributed assembly tests pass `2/2`; Angular tests pass `5/5` across `3/3` files.
- The complete .NET solution and Angular production application build with zero warnings/errors. `lg db check` reports no model drift, and no EF migration file changed. Production npm audit reports zero vulnerabilities. OperationKey search finds every key across the intended layers, and the targeted legacy-route scan finds no match in `src`, `tests`, or `web`.
- The explicitly enabled Aspire scenario is currently `BLOCKED`, not passed: Docker client/server checks succeed, but ephemeral PostgreSQL connections fail with `NpgsqlException`/`EndOfStreamException` and NATS times out before `portfolio-api` becomes healthy. A direct Portfolio API startup on loopback succeeds, so current evidence identifies local container dependency connectivity rather than an API startup or route-registration failure. The full canonical Gateway/PostgreSQL flow must be rerun after that environment issue is repaired before this plan moves out of `active`.

## Documentation rule

Each implementation milestone updates this plan and at least one relevant architecture, quality, or project-state document in the same task. Change `Status: Planned` only when implementation begins, and mark the plan complete only after all completion criteria have direct evidence.
