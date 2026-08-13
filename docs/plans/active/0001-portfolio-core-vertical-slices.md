# Distributed Foundation and Portfolio Core Vertical Slices

Last reviewed: 2026-08-12. Status: In implementation.

## Goal

First prove the complete Aspire-distributed project graph without business logic. Only after the foundation gate passes, deliver the trustworthy portfolio core incrementally. No milestone introduces real user data or secrets into Git.

## Milestone 0 — toolchain and decision gate

**Tasks:** pin and restore project-local .NET 10, Aspire 13.4, Node/npm, PostgreSQL image and dependency versions; verify Docker client/server; accept ADR-0015 and synthetic calculation fixtures.  
**Acceptance:** wrappers resolve only the pinned local toolchain; bootstrap is idempotent; Docker daemon and Compose are healthy; all direct dependencies have approved free-use licenses.  
**State:** local .NET/Aspire/Node/npm are restored; Docker Desktop, Compose, and WSL are verified healthy. The remaining Milestone 0 item is owner approval of direct-dependency free-use licenses.

## Milestone 1 — complete distributed project foundation (no business logic)

**Tasks:** create Angular connectivity shell; ASP.NET Core/YARP Gateway; Portfolio Core, Market Data, Slip Import API/worker, Research, and Operations Clean Architecture projects; ServiceDefaults; IntegrationContracts transport envelope; AppHost; one PostgreSQL server with five databases; NATS JetStream; per-service `DbContext`/design factory; architecture, gateway, and Aspire distributed-app tests.  
**Acceptance:** one command starts the complete graph; every resource becomes healthy; Gateway reaches every API through logical service discovery; Angular calls Gateway only; each service receives only its database and NATS reference; dynamic ports work; Aspire Dashboard displays logs/traces/metrics; nullable/analyzers/build/tests/privacy scans pass; no business type/table/event exists; no `EnsureCreated`, cross-service project/database reference, mediator, mapper, validation, generic-repository, or extra unit-of-work framework exists.  
**Security/performance:** only Angular/Gateway are browser-facing on loopback; benchmark cold/warm startup and local memory; runtime volumes stay outside the repository.

**State:** Complete on 2026-08-12. The 16-resource Dashboard view showed every long-running resource in `Running` state and the installer resource finished; the Angular shell reported all five services connected. Aspire Dashboard directly showed Gateway-to-backend traces, structured logs with trace identifiers, and Gateway HTTP request-duration metrics for the five routed paths. A clean, explicitly enabled distributed-app run passed both tests in 33 seconds. Cold-process readiness was observed within 26.743 seconds; one warm restart took 32.178 seconds, which is above the proposed 30-second target but is not a p95 sample. The attributed steady-state local working set was approximately 973.5 MiB including PostgreSQL and NATS. Builds, component/integration/architecture tests, and targeted privacy/invariant scans pass. Persistent PostgreSQL credentials remain outside Git in AppHost .NET User Secrets; distributed tests use ephemeral credentials and no named volumes. See [Foundation Benchmark Results](../../quality/FOUNDATION_BENCHMARK_RESULTS.md).

## Milestone 2 — fixed user, portfolio and auditable ledger

Implement `ICurrentUser`, externally configured fixed user, `UserProfile`, required Portfolio ownership, deposits, withdrawals, buys, sells, fees, taxes, dividends, corrections, Code First migrations and minimal UI inside Portfolio Core. Generate every LedgerLens-owned domain and persistent identifier as UUIDv7 with `Guid.CreateVersion7()` and reject non-v7 UUID resource identifiers at API boundaries. Preserve one local transaction for financial invariants.

## Milestone 3 — lots, holdings and dual cost views

Implement lot creation/allocation, FIFO suggestion, average-cost projection, holdings, rebuild/checkpoint and reconciliation in Portfolio Core.

## Milestone 4 — Market Data FX and THB attribution

Implement Market Data FX/provider adapters, provenance/freshness, Outbox/Inbox, versioned NATS integration, Portfolio Core rate projection, and exact THB attribution.

## Milestone 5 — dashboard and coordinated backup/restore

Implement incomplete-state dashboard composition plus Operations-coordinated write pause, Outbox drain, service-owned backups, document checksum manifest, and clean restore.

## Milestone 6 — quotes, watchlist and performance

Implement quotes/candles/provider quotas in Market Data and activate Research watchlists when required. Validate provider terms before acceptance.

## Milestone 7 — Dime import

Implement Slip Import documents, extraction worker, review, duplicate detection, idempotent Portfolio Core posting, and synthetic parser benchmarks after ADR-0009 evidence passes.

Every completed milestone updates `PROJECT_STATE.md`, risks, dependency inventory, verified commands, reconciliation evidence, and relevant ADRs.
