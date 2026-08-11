# Project State

Last reviewed: 2026-08-11.

## Current state

- The distributed foundation source is implemented: AppHost, ServiceDefaults, IntegrationContracts, YARP Gateway, five coarse-grained service skeletons, Slip Import worker, five service-owned `DbContext` types, Angular connectivity shell, PostgreSQL/NATS topology, and foundation tests.
- The complete .NET solution builds with zero warnings/errors; four architecture tests and two Gateway tests pass, including Gateway-to-five-ephemeral-services routing on dynamic ports; Angular production build and two component tests pass; privacy/business-table scans pass.
- The runtime foundation gate is not yet complete. Docker Desktop could not start in this session because another Windows user owns `dockerExtensionManagerAPI`; therefore container readiness, JetStream persistence, Dashboard telemetry, and Gateway-to-service connectivity remain to be executed once the local Docker engine is available.
- Product scope, proposed architecture, research evidence, ADRs, quality gates, risks, and the first implementation plan are documented.
- The source planning context remains at `../LedgerLens_Codex_Planning_Context.md` relative to the LedgerLens folder.

## Accepted direction

.NET 10 LTS, Aspire 13.4 stable, Node 24 LTS, Angular 22.x with project-local Tailwind CSS and no Bootstrap UI dependency, PostgreSQL 18.x, EF/Npgsql 10.x; project-local SDK/CLI tooling with Docker as the system runtime; coarse-grained Gateway, Portfolio Core, Market Data, Slip Import, Research, and Operations services; per-service Clean Architecture and database ownership; HTTP/JSON through Gateway and NATS JetStream for future asynchronous integration; Aspire Dashboard observability; explicit handlers and no mediator package; EF Core Code First; free dependencies only; loopback-only; no business logic until the distributed foundation gate passes.

## Tooling readiness

- Project-local .NET SDK 10.0.302, Aspire CLI 13.4.6, Node 24.19.0, and npm 11.17.0 are pinned by `toolchain.json` and restored under ignored project-local paths.
- The repository-scoped `thefools` development CLI wraps the existing project-local commands. Its PowerShell profile function supports bare `thefools run` only from inside the repository tree and does not modify the user or machine `PATH`.
- Docker CLI 29.7.2 is installed. Docker Desktop startup on 2026-08-11 failed with `dockerExtensionManagerAPI: Access is denied` because another Windows user/session owns the named pipe; no other user's process was modified.
- The accepted design installs .NET, Aspire, Node/Angular CLI, and EF tooling per project without permanent `PATH` changes. See [Local Development Tooling](LOCAL_DEVELOPMENT_TOOLING.md).

## Gated decisions

- Market provider: Proposed pending terms and prototype.
- Dime extraction engine/parser: Needs Evidence pending private sample evaluation.
- Numeric scales/performance budgets: Proposed pending executable fixtures and measurements.
- MIT license: Proposed pending owner confirmation.

## Next work

Make the local Docker engine available, run `.\scripts\dev.ps1 run`, and execute the opt-in distributed test with `LEDGERLENS_RUN_DISTRIBUTED_TESTS=1`. Do not begin Milestone 2 until PostgreSQL, NATS persistence, all resource health, Dashboard telemetry, dynamic-port routing, and Gateway-to-service calls pass.
