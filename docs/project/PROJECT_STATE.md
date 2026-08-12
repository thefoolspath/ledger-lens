# Project State

Last reviewed: 2026-08-12.

## Current state

- The distributed foundation source is implemented: AppHost, ServiceDefaults, IntegrationContracts, YARP Gateway, five coarse-grained service skeletons, Slip Import worker, five service-owned `DbContext` types, Angular connectivity shell, PostgreSQL/NATS topology, and foundation tests.
- The complete .NET solution builds with zero warnings/errors; four architecture tests and two Gateway tests pass, including Gateway-to-five-ephemeral-services routing on dynamic ports; Angular production build and two component tests pass; privacy/business-table scans pass.
- Docker Desktop and WSL are healthy. The complete 15-resource graph becomes healthy; Gateway reaches all five APIs on dynamic ports; Angular reaches those APIs through its Gateway proxy; PostgreSQL 18.4 and NATS JetStream use named volumes outside the repository; and a stop/start cycle reuses those volumes without PostgreSQL authentication failures.
- Persistent local runs require `Parameters:postgres-password` in the AppHost .NET User Secrets store (`UserSecretsId` `ledgerlens-apphost-development`). Distributed tests disable named volumes and use a generated ephemeral password so parallel/repeated tests cannot collide with developer data.
- The runtime foundation gate remains open only for direct Aspire Dashboard confirmation of correlated logs/traces/metrics and the documented cold/warm startup and local-memory measurements. No business milestone has started.
- Product scope, proposed architecture, research evidence, ADRs, quality gates, risks, and the first implementation plan are documented.
- The source planning context remains at `../LedgerLens_Codex_Planning_Context.md` relative to the LedgerLens folder.

## Accepted direction

.NET 10 LTS, Aspire 13.4 stable, Node 24 LTS, Angular 22.x with project-local Tailwind CSS and no Bootstrap UI dependency, PostgreSQL 18.x, EF/Npgsql 10.x; project-local SDK/CLI tooling with Docker as the system runtime; coarse-grained Gateway, Portfolio Core, Market Data, Slip Import, Research, and Operations services; per-service Clean Architecture and database ownership; HTTP/JSON through Gateway and NATS JetStream for future asynchronous integration; Aspire Dashboard observability; explicit handlers and no mediator package; EF Core Code First; free dependencies only; loopback-only; no business logic until the distributed foundation gate passes.

## Tooling readiness

- Project-local .NET SDK 10.0.302, Aspire CLI 13.4.6, Node 24.19.0, and npm 11.17.0 are pinned by `toolchain.json` and restored under ignored project-local paths.
- The repository-scoped `lg` development CLI wraps the existing project-local commands. Its PowerShell profile function supports bare `lg run` only from inside the repository tree and does not modify the user or machine `PATH`.
- Docker client/server 29.7.2, Docker Desktop 4.86.0, Docker Compose 5.3.1, and WSL 2.7.11.0 are verified healthy on 2026-08-12.
- The accepted design installs .NET, Aspire, Node/Angular CLI, and EF tooling per project without permanent `PATH` changes. See [Local Development Tooling](LOCAL_DEVELOPMENT_TOOLING.md).

## Gated decisions

- Market provider: Proposed pending terms and prototype.
- Dime extraction engine/parser: Needs Evidence pending private sample evaluation.
- Numeric scales/performance budgets: Proposed pending executable fixtures and measurements.
- MIT license: Proposed pending owner confirmation.

## Next work

Verify one correlated Angular-to-Gateway-to-backend request in Aspire Dashboard logs/traces/metrics, record cold/warm startup and local-memory measurements, and rerun the final build/test/privacy gates. Do not begin Milestone 2 until those remaining Milestone 1 acceptance checks pass.
