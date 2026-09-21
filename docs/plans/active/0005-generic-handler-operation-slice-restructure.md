# Generic Handler and Operation-Slice Restructure

Last reviewed: 2026-09-21. Status: Implemented; distributed runtime re-verification is blocked by the local Docker daemon.

## Goal

Make the direct endpoint-to-handler architecture compile-time explicit and make a route-derived `OperationKey` lead to the complete C# use case without introducing a mediator, runtime dispatcher, reflection scanning, or generic repository.

## Implemented structure

- Added the shared `LedgerLens.Application.Abstractions` project with `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>`.
- Converted all 18 Portfolio Core and Market Data operations to exact closed-generic handler contracts: nine commands and nine queries.
- Organized API request/endpoint files and Application command/query/handler files under `Features/{Feature}/{OperationKey}` with matching namespaces.
- Kept endpoint composition files as thin feature-group aggregators and preserved all routes, endpoint names, status codes, envelopes, validation, ownership, transaction, provider, and financial behavior.
- Registered every closed generic handler explicitly with scoped lifetime. Assembly scanning, reflection dispatch, `IServiceProvider` dispatch, mediator packages, and generic repositories remain prohibited.
- Classified `SimulationValuationsCalculateSeriesList` as a query because it is a non-persisting calculation even though transport compatibility uses HTTP POST.
- Moved input whitespace canonicalization to Angular request construction. Backend Domain/Application code validates received values but no longer calls `.Trim()` or silently rewrites whitespace.

## Verification evidence

- `./scripts/dev.ps1 build`: passed the complete .NET solution with zero warnings/errors and the Angular production build.
- Architecture tests: `19/19` passed, including exact handler counts, generic contract implementation, explicit registrations, exact endpoint injection, and prohibited-dispatch checks.
- Portfolio Core unit tests: `11/11` passed. Market Data unit tests: `2/2` passed. Gateway integration tests: `3/3` passed.
- The normal full-solution test command passed, including the two guarded DistributedAppTests assembly checks; test-only placeholder assemblies currently report no discoverable tests as before.
- Angular tests: `10/10` passed across four files, including the final request-body assertion that proves user-entered text is trimmed before transmission.
- Production `npm audit --omit=dev`: zero vulnerabilities.
- `lg db check` with a synthetic design-time-only localhost connection string: no pending model changes.
- Backend `.Trim()` scan: no matches under `src/Services`.
- The explicitly enabled Aspire distributed test could not start because the Docker Desktop Linux daemon pipe was absent; `docker info` confirmed that only the client was available. This is an environment blocker, not an application assertion failure.

## Closeout condition

Move this plan to `docs/plans/completed/` after one successful explicitly enabled Aspire distributed runtime pass on a healthy Docker daemon. The final Angular/architecture reruns and diff checks already pass; no code change is required merely to clear the remaining external runtime gate.
