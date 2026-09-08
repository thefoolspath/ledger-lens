# Project State

Last reviewed: 2026-09-08.

## Current state

- The distributed foundation source is implemented: AppHost, ServiceDefaults, IntegrationContracts, YARP Gateway, five coarse-grained services, Slip Import worker, five service-owned `DbContext` types, Angular shell, PostgreSQL/NATS topology, and foundation tests.
- The complete .NET solution and Angular production application build with zero warnings/errors. The 2026-09-08 closeout passed 16 architecture tests, ten Portfolio Core unit tests, two Gateway tests, one Market Data unit test, two explicitly enabled distributed tests, five Angular tests, EF model-drift checking, production npm audit, OperationKey/legacy-route scans, and tracked privacy checks.
- The current ephemeral Aspire graph reaches healthy PostgreSQL 18.4 and NATS JetStream resources, applies the complete Portfolio Core migration chain, and exercises the canonical Gateway routes with synthetic data. AppHost gives command-line test parameters precedence over developer User Secrets, preventing local configuration from contaminating distributed runs.
- Persistent local runs require `Parameters:postgres-password`, `Parameters:fixed-user-id`, and `Parameters:fixed-user-email` in the AppHost .NET User Secrets store (`UserSecretsId` `ledgerlens-apphost-development`). Distributed tests disable named volumes and use a generated ephemeral password plus synthetic fixed-user configuration so parallel/repeated tests cannot collide with developer data.
- Milestone 1 is complete. Direct Aspire Dashboard inspection verified the foundation resource view, five connected Angular service cards, Gateway-to-backend distributed traces, trace-linked structured logs, and Gateway request metrics. The recorded process-start observations were at most 26.743 seconds for the first measured start and 32.178 seconds for one warm restart; attributed steady-state local working set was approximately 973.5 MiB including PostgreSQL and NATS. The warm sample is above the proposed 30-second target and remains an investigation data point, not a p95 result.
- Milestone 0 is closed: on 2026-08-26 the owner approved the free-use licenses for the admitted direct application, development, and test dependency baseline. Candidate dependencies and external provider/data terms are not implicitly approved.
- Milestone 2 is in implementation. Its first vertical slice is verified across Angular, Gateway, Portfolio Core, EF Core migration, and ephemeral PostgreSQL: fixed current-user configuration, `UserProfile`, required Portfolio ownership, Investment Accounts, Deposit/Withdrawal entries, and a ledger-derived exact cash balance are implemented. Buys, sells, fees, taxes, dividends, corrections, and their reconciliation coverage remain.
- Portfolio Core now models eight business tables: the four confirmed-ledger tables plus isolated `simulation_accounts`, `simulation_trade_drafts`, `simulation_trade_entries`, and `simulation_valuation_snapshots`. LedgerLens-owned identifiers are generated as UUIDv7 and route resource identifiers are rejected when they are not UUIDv7. The complete migration chain, including the four simulation tables, is verified against ephemeral PostgreSQL.
- Portfolio Core pilots the accepted service-owned hybrid database workflow: generated persistence types and `PortfolioCoreDbContext` are isolated in `LedgerLens.PortfolioCore.Database`; Infrastructure maps persistence records to Domain; the one-shot Migrations project owns the preserved migration IDs and snapshot; and guarded `lg db` commands support approved DB-first scaffolding, migration generation, drift checking, and reviewed SQL without automatic database/history writes.
- Product scope, proposed architecture, research evidence, ADRs, quality gates, risks, and the first implementation plan are documented.
- Manual Simulation Accounts are implemented through provider-neutral Market Data APIs, a deterministic Development/test provider, a terms/key-gated Twelve Data prototype adapter, separate Portfolio Core aggregates/tables and migration, idempotent preview/confirm plus reversal/replacement correction APIs, FIFO/average/simple-return and USD/THB valuation calculations, and a separate Angular Simulation mode. Apache ECharts 6.1.0 is directly lazy-loaded for value/cost and OHLCV views with ARIA and table fallbacks. Code-level, unit, component, build, migration-model, privacy, dependency, and PostgreSQL end-to-end isolation gates pass; desktop/mobile visual QA remains pending.
- The owner-approved FX reporting policy now separates trade-date stock attribution from actual exchange and USD-cash effects. It uses auditable USD cash lots with evidence/manual/FIFO allocation, parallel primary and BOT benchmark columns, and a non-overlapping bridge that must reconcile a closed cycle to actual THB proceeds minus contributions.
- Plan 0003 is complete across all 18 current Portfolio Core and Market Data business operations: canonical Version 1 routes, unique endpoint names, operation-keyed transport/application/persistence identifiers, Angular route constants, repository-owned scenarios, one-public-type-per-file organization, naming/cardinality architecture tests, and the explicitly enabled Aspire/PostgreSQL/NATS flow all pass without compatibility aliases or EF schema changes. The plan is archived under `docs/plans/completed/`.
- The source planning context remains at `../LedgerLens_Codex_Planning_Context.md` relative to the LedgerLens folder.

## Accepted direction

.NET 10 LTS, Aspire 13.4 stable, Node 24 LTS, Angular 22.x with project-local Tailwind CSS and no Bootstrap UI dependency, PostgreSQL 18.x, EF/Npgsql 10.x; project-local SDK/CLI tooling with Docker as the system runtime; coarse-grained Gateway, Portfolio Core, Market Data, Slip Import, Research, and Operations services; per-service Clean Architecture and database ownership; HTTP/JSON through Gateway and NATS JetStream for future asynchronous integration; Aspire Dashboard observability; explicit handlers and no mediator package; reviewed EF migrations with a guarded Portfolio Core hybrid DB-first pilot; free dependencies only; loopback-only; no business logic until the distributed foundation gate passes; trade-date stock FX plus separately reconciled actual exchange/USD-cash FX with primary and BOT benchmark views.

## Tooling readiness

- Project-local .NET SDK 10.0.302, Aspire CLI 13.4.6, Node 24.19.0, and npm 11.17.0 are pinned by `toolchain.json` and restored under ignored project-local paths.
- The repository-scoped `lg` development CLI wraps the existing project-local commands. Its PowerShell profile function supports bare `lg run` only from inside the repository tree and does not modify the user or machine `PATH`.
- Docker client/server 29.7.2, Docker Desktop 4.86.0, and Docker Compose 5.3.1 are verified healthy on 2026-09-08; WSL 2.7.11.0 was last verified on 2026-08-12.
- The accepted design installs .NET, Aspire, Node/Angular CLI, and EF tooling per project without permanent `PATH` changes. See [Local Development Tooling](LOCAL_DEVELOPMENT_TOOLING.md).

## Gated decisions

- Market provider: Proposed pending terms and prototype.
- Dime extraction engine/parser: Needs Evidence pending private sample evaluation.
- Numeric scales/performance budgets: Proposed pending executable fixtures and measurements.
- MIT license: Proposed pending owner confirmation.

## Next work

Complete desktop/mobile visual QA for the manual Simulation mode, then close plan 0002 if its remaining acceptance evidence passes. Continue confirmed-ledger Milestone 2 work without reusing simulation tables or projections. Keep the 32.178-second warm-start observation visible while collecting enough repeat runs to estimate p95.
