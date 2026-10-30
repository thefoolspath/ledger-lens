# Generic Handler and Operation-Slice Restructure

Last reviewed: 2026-10-30. Status: Completed; distributed runtime re-verification passed on 2026-10-25.

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
- The earlier explicitly enabled Aspire distributed test could not start because the Docker Desktop Linux daemon pipe was absent. After recovery, the explicitly enabled distributed suite passed `2/2` on 2026-10-25 with ephemeral PostgreSQL and NATS, as recorded in `docs/project/PROJECT_STATE.md` and the branch's 2026-10-25 verification commit.

## Closeout condition

The distributed runtime closeout condition is satisfied. The previously verified Angular, architecture, and diff gates remain the evidence for this plan; no new implementation is claimed by this documentation closeout.
