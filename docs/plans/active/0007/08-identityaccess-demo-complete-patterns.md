# 0007.08 — IdentityAccessDemo pattern coverage

Status: In implementation. The Excel utility remains unconnected; the first failure-boundary and route/Location stages are in source. Last reviewed: 2026-11-16.

Implementation progress (2026-11-13): `UsersCreate` and `UsersGetOne` now use the `/api/identity-demo` Gateway route, `/identity-access-demo` Angular route, `identity-demo-*` Aspire resources, and a new `ledgerlens_identity_demo` database name. The old database is not migrated or deleted. Backend projects, namespaces, and service/test folders are named `IdentityAccessDemo`. `UsersCreate` emits a service-local `/v1/users/get-one/{id}` Location, and the Gateway applies its validated Location rewrite to the demo route. The API restricts creates to `@example.test`; distinct create-conflict codes and safe malformed-JSON handling have focused tests. The AppHost graph and Angular production bundle build; Gateway tests passed 15/15, IdentityAccessDemo unit tests 5/5, HTTP tests 10/10, architecture tests 20/20, Angular tests 12/12, and the non-Docker Aspire resource-model test 1/1. `lg docs` and `git diff --check` passed at that stage. Docker was subsequently started and the explicitly enabled IdentityAccessDemo distributed test passed 1/1 against ephemeral PostgreSQL through Web and Gateway. A focused Gateway `500` pass-through test also passed 1/1. Browser verification of the renamed route, page/import/export, and their remaining evidence gates are still open.

On 2026-11-14, the expanded 0007.02 HTTP suite passed 12/12, including invalid handler result shape, allowlisted operation logging, and the absence of application Error logs for expected failures.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md). This is a scoped IdentityAccessDemo implementation plan. It does not close the system-wide 0007 decisions or authorize plan 0006 authentication.

## Owner decisions

- Rename the existing IdentityAccess demonstration in place to `IdentityAccessDemo` across backend projects/namespaces, Aspire resource names, Gateway route, database name, and Angular feature/route. Start with a new empty demo database; do not migrate or delete the existing IdentityAccess database.
- Keep the Gateway and all supporting services loopback-only. The demo remains unauthenticated and uses synthetic data only. Create and import accept only `@example.test` email addresses; `DisplayName` remains unrestricted, with UI guidance to use invented names. Do not claim owner authorization or suitability for real identity data.
- The current `UsersCreate` and `UsersGetOne` remain the first vertical slice. After the Gateway rename, `UsersCreate` returns `201` with the shared envelope and a canonical `/api/identity-demo/v1/users/get-one/{id}` `Location` that can be followed through the Gateway.
- Add `UsersGetPage` with server-side count and separate Email and DisplayName filters. Each filter uses case-insensitive contains matching; apply both with AND when both are supplied. Sort deterministically by `createdAt` or normalized email in either direction, default to newest first, and use ID as a unique tie-breaker. Start at 10 rows per page; the UI offers 10, 50, 100, and 500 rows. The owner selected 500 as an option, but enabling it is gated on representative query and UI measurements because 0007.04 previously proposed 100 as the candidate maximum. Echo effective pagination and sorting metadata. This is a demo-specific collection; it does not replace the Portfolio/Simulation migrations in 0007.04.
- Add a demo XLSX import with `Email` and `DisplayName` columns and a downloadable template. Read the first worksheet, as the copied WMS importer does; other worksheets are outside the import scope and the UI must state this. Generate user IDs and operation IDs server-side. Validate the whole worksheet before an atomic create-only commit; any invalid or duplicate cell rejects the entire file. Report sheet/cell/message in the page and provide an annotated error workbook for immediate download only, without retaining that workbook on the server after the response. A valid file commits on the Import action without a preview/confirm step. On success, show the number of users created and reload the current page using its filters and sort; do not return the full imported user list. Design retries to reconcile a lost response without duplicating users.
- Add synchronous and asynchronous XLSX export as two explicit page actions. Export the complete filtered/sorted result, with `Email`, `DisplayName`, and `CreatedAt` columns; omit ID and internal persistence columns. The asynchronous job has status, explicit cancellation, download, restart recovery, and a 24-hour completed-file expiry. Remember demo job references in browser storage so the page can resume status/download after refresh or reopening. Files and temporary artifacts stay outside the repository. Browser refresh does not cancel an accepted job.
- Use an IdentityAccessDemo feature route, route-scoped state, cancellation of superseded reads, and SCSS/tokens/shared controls demonstrated by this feature. Leave existing Portfolio/Simulation UI extraction and app-wide Tailwind removal for their own implementation stages.
- Use narrow persistence ports per operation. Keep import transaction ownership in one cohesive boundary; no generic repository or cross-context transaction.

## Coverage of the seven decisions

| Decision | IdentityAccessDemo implementation target | System-wide boundary |
| --- | --- | --- |
| 0007.01 | Truthful `UsersCreate` Location through Gateway | Existing Portfolio policy remains as implemented |
| 0007.02 | Typed validation/conflict failures, safe unexpected-error handling, and import/page/job errors | Plan 0004 rollout to other services remains open |
| 0007.03 | Consistent `IdentityAccessDemo` and operation vocabulary in backend, UI, tests, and docs | System-wide naming inventory remains open |
| 0007.04 | `UsersGetPage`, effective page/sort metadata, stable server-side ordering | Simulation >5,000-entry distributed gate and other pageable collections remain open |
| 0007.05 | Feature-local Angular state and an evidence-backed SCSS foundation | Whole-app extraction and Tailwind removal remain open |
| 0007.06 | Cancellation, import retry reconciliation, synchronous XLSX and durable async XLSX job lifecycle | Financial mutation policies and owner-bound export authorization await separate work/plan 0006 |
| 0007.07 | Operation-specific ports and one service-owned EF context | Existing Portfolio Core store decomposition remains open |

## First implementation stage: copy the WMS Excel utility

The owner requested a copy before further utility behavior changes. `src/LedgerLens.Spreadsheets/` contains the relevant WMS `CreateReport.CreateExcel` implementation from `Share/UtilityLibrary/Utility.cs`, `BaseReportResponse`, `AppDateTime`, and the ExcelImporter implementation with its supporting import, response, validation, mapping, and error classes. The source reference is `C:/Users/adeel/OneDrive/Documents/Work Space/Codding WorkSpace/RIS/WMS-SERVICE-BACKEND/Share/UtilityLibrary/`. The Location import repository was inspected as a usage example and is not copied. The utility uses ClosedXML `0.104.2`, matching WMS. The copy was extracted into a separate project, omitted unrelated PDF/Location methods, and removed an unused Newtonsoft import for compilation; Excel method behavior has not been intentionally changed.

This copied utility is not yet wired into IdentityAccessDemo. Its WMS return types, reflection mapping, status code, full-workbook memory behavior, date/time naming, error text, and workbook styling are retained as source behavior pending the owner's later change list. Do not expose it through an endpoint or call it with unbounded data before review. Import must not write a database entity directly from workbook reflection.

The copied WMS files use block-scoped namespaces, while LedgerLens enforces file-scoped namespaces. Code-style enforcement is disabled only for this unconnected project during the copy stage; restore the repository rule when adapting the utility.

The one-public-type-per-file architecture check also excludes only this unconnected copy during the copy stage because the original `CreateReport.Excel.cs` file contains `CreateReport` and nested utility types. Restore the check when the utility is adapted.

## Evidence gates before the next stage

- Review the owner's forthcoming utility changes, then decide the LedgerLens transport-independent Excel interface and how import errors are returned as Problem Details plus an annotated workbook download.
- Benchmark synthetic XLSX import and export at multiple sizes, recording cold/warm time, peak memory, allocation, file size, cancellation/cleanup, and run count under `docs/quality/BENCHMARK_PLAN.md`. Measure the 500-row page option against representative synthetic data and the current query/index shape before enabling it. Ask the owner to choose the import file/row ceiling and named request/job deadlines from that evidence; no arbitrary threshold is approved yet.
- Review ClosedXML license, transitive packages, advisories, resource cost, and alternatives under the dependency admission policy before an API or worker takes a runtime dependency on this project.
- For each vertical stage, update the owning 0007 child record, relevant architecture/quality docs, and `docs/project/PROJECT_STATE.md` after behavior is verified. Run focused tests, production builds, docs checks, privacy scans, `git diff --check`, and ephemeral PostgreSQL/Gateway/browser scenarios where relevant.
