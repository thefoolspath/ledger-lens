# Distributed Foundation and Portfolio Core Vertical Slices

Last reviewed: 2026-09-01. Status: In implementation.

## Goal

First prove the complete Aspire-distributed project graph without business logic. Only after the foundation gate passes, deliver the trustworthy portfolio core incrementally. No milestone introduces real user data or secrets into Git.

## Milestone 0 — toolchain and decision gate

**Tasks:** pin and restore project-local .NET 10, Aspire 13.4, Node/npm, PostgreSQL image and dependency versions; verify Docker client/server; accept ADR-0015 and synthetic calculation fixtures.  
**Acceptance:** wrappers resolve only the pinned local toolchain; bootstrap is idempotent; Docker daemon and Compose are healthy; all direct dependencies have approved free-use licenses.  
**State:** Complete on 2026-08-26. Local .NET/Aspire/Node/npm are restored; Docker Desktop, Compose, and WSL are verified healthy. The owner approved the free-use licenses for the admitted direct application, development, and test dependency baseline. Candidate dependencies and provider/data terms still require separate admission.

## Milestone 1 — complete distributed project foundation (no business logic)

**Tasks:** create Angular connectivity shell; ASP.NET Core/YARP Gateway; Portfolio Core, Market Data, Slip Import API/worker, Research, and Operations Clean Architecture projects; ServiceDefaults; IntegrationContracts transport envelope; AppHost; one PostgreSQL server with five databases; NATS JetStream; per-service `DbContext`/design factory; architecture, gateway, and Aspire distributed-app tests.  
**Acceptance:** one command starts the complete graph; every resource becomes healthy; Gateway reaches every API through logical service discovery; Angular calls Gateway only; each service receives only its database and NATS reference; dynamic ports work; Aspire Dashboard displays logs/traces/metrics; nullable/analyzers/build/tests/privacy scans pass; no business type/table/event exists; no `EnsureCreated`, cross-service project/database reference, mediator, mapper, validation, generic-repository, or extra unit-of-work framework exists.  
**Security/performance:** only Angular/Gateway are browser-facing on loopback; benchmark cold/warm startup and local memory; runtime volumes stay outside the repository.

**State:** Complete on 2026-08-12. The 16-resource Dashboard view showed every long-running resource in `Running` state and the installer resource finished; the Angular shell reported all five services connected. Aspire Dashboard directly showed Gateway-to-backend traces, structured logs with trace identifiers, and Gateway HTTP request-duration metrics for the five routed paths. A clean, explicitly enabled distributed-app run passed both tests in 33 seconds. Cold-process readiness was observed within 26.743 seconds; one warm restart took 32.178 seconds, which is above the proposed 30-second target but is not a p95 sample. The attributed steady-state local working set was approximately 973.5 MiB including PostgreSQL and NATS. Builds, component/integration/architecture tests, and targeted privacy/invariant scans pass. Persistent PostgreSQL credentials remain outside Git in AppHost .NET User Secrets; distributed tests use ephemeral credentials and no named volumes. See [Foundation Benchmark Results](../../quality/FOUNDATION_BENCHMARK_RESULTS.md).

## Milestone 2 — fixed user, portfolio and auditable ledger

Implement `ICurrentUser`, externally configured fixed user, `UserProfile`, required Portfolio ownership, deposits, withdrawals, buys, sells, fees, taxes, dividends, corrections, reviewed EF migrations and minimal UI inside Portfolio Core. Generate every LedgerLens-owned domain and persistent identifier as UUIDv7 with `Guid.CreateVersion7()` and reject non-v7 UUID resource identifiers at API boundaries. Preserve one local transaction for financial invariants.

**State:** In implementation. The first verified slice now supplies externally configured `FixedLocalCurrentUser`, idempotent `UserProfile` bootstrap during portfolio creation, required owner-filtered Portfolio access, Investment Account creation, append-only Deposit and Withdrawal entries using `numeric(28,10)`, ledger-derived cash balance, UUIDv7 generation/API validation, an explicit Portfolio Core migration resource, loopback-origin mutation protection, and a minimal Angular workflow. Portfolio Core now pilots ADR-0016: generated persistence and `DbContext` are isolated in its service-owned Database project, migrations and snapshot are owned by the one-shot Migrations project, and guarded `lg db` commands support DB-first scaffold, migration generation, drift checking, and SQL review without automatic apply/history writes. The distributed acceptance test applies the migration to ephemeral PostgreSQL and reconciles synthetic `100.50 - 25.00 = 75.50` through Gateway. Buys, sells, fees, taxes, dividends, correction/reversal semantics, and their UI/tests remain before Milestone 2 can be complete.

## Milestone 3 — lots, holdings and dual cost views

Implement lot creation/allocation, FIFO suggestion, average-cost projection, holdings, rebuild/checkpoint and reconciliation in Portfolio Core.

## Milestone 4 — Market Data FX and THB attribution

Implement Market Data FX/provider adapters, provenance/freshness, Outbox/Inbox, versioned NATS integration, Portfolio Core primary/BOT rate projections, USD cash lots with evidence/manual/FIFO allocation and versioned corrections, trade-date stock attribution, non-overlapping cash/stock/exchange THB bridge, and exact closed-cycle reconciliation. Preserve trade and settlement timestamps, carry basis through linked internal USD transfers, and expose incomplete state for missing rates, allocation, or external-USD basis.

## Milestone 5 — dashboard and coordinated backup/restore

Implement incomplete-state dashboard composition plus Operations-coordinated write pause, Outbox drain, service-owned backups, document checksum manifest, and clean restore.

## Milestone 6 — quotes, watchlist and performance

Implement quotes/candles/provider quotas in Market Data, retain the completed isolated manual Simulation Account slices in [plan 0002](../completed/0002-simulation-portfolio.md), and activate Research watchlists when required. Validate provider terms before accepting an external adapter.

## Milestone 7 — Dime import

Implement Slip Import documents, extraction worker, review, duplicate detection, idempotent Portfolio Core posting, and synthetic parser benchmarks after ADR-0009 evidence passes.

Every completed milestone updates `PROJECT_STATE.md`, risks, dependency inventory, verified commands, reconciliation evidence, and relevant ADRs.
