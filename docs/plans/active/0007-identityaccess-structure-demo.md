# Plan 0007 — IdentityAccess structure demonstration

Status: Scoped demonstration verified; system-wide plan 0007 remains open. Last reviewed: 2026-11-11.

## Owner scope

Apply the relevant structural decisions from the entire 0007 decision set to one new IdentityAccess service and one Angular preview feature. Limit the backend to two operations: `UsersCreate` command and `UsersGetOne` query. Do not migrate existing business services until the owner explicitly asks. Preserve the uncommitted 0007.02 work separately on branch `codex/0007-02-typed-failures` in a named stash.

This demonstration does not accept ADR-0019 through ADR-0021 or implement plan 0006 authentication. IdentityAccess remains internal; there is no Gateway route for user administration. The Angular route now sends synthetic user requests to the internal backend through the Aspire local development proxy; no Gateway route or authentication flow is added.

## Decision coverage

| Item | IdentityAccess demonstration | Boundary |
| --- | --- | --- |
| 0007.01 Created/Location | `UsersCreate` returns `201` and the shared envelope without `Location`. | No safe public Gateway retrieval URL exists. |
| 0007.02 Typed failures | Invalid input and duplicate email are operation-specific typed results; the API maps them to Problem Details. Unexpected errors reach an API-local exception handler with allowlisted structured log fields and generic `500`. | Full shared 0004/0007.02 rollout remains stashed and unmerged. |
| 0007.03 Naming | Plural `Users` feature, `Create`/`GetOne` operations, explicit handler and store names. | No existing-service rename. |
| 0007.04 Collections | No pageable collection exists in these two operations. | Pagination/sorting and the existing Simulation correctness gate remain separate work. |
| 0007.05 Angular/SCSS | Lazy `identity-demo` feature route, colocated page/model/HTTP client, local SCSS and global demo tokens/form rules. | Existing Portfolio/Simulation root extraction, complete global SCSS migration, and Tailwind removal remain open. |
| 0007.06 Lifecycle | Query cancellation token flows endpoint → handler → store → EF. Create takes a UUIDv7 request ID, stores it uniquely, and replays the same user on a retry with matching input; the token flows through the write. | Timeout values, financial mutation handoff, and exports require their own evidence/use cases. |
| 0007.07 Persistence ports | Create and GetOne handlers depend on separate narrow ports; one scoped EF context remains service-owned. | No generic repository or existing-service store split. |

## Acceptance and evidence

- Generated EF migration creates only the service-owned `users` table and unique normalized-email/request-ID indexes. Review SQL and apply against ephemeral PostgreSQL before claiming database runtime or idempotency behavior.
- The API, migration runner, AppHost and Angular production app build. Run handler tests and Angular tests. Verify `git diff --check`, docs links, and privacy scan.
- Inspect the `/identity-demo` route in a browser before claiming visual or responsive acceptance.
- No real identity data, credentials, or Google tokens enter tracked files or demo fixtures.

## Remaining gates

The whole 0007 decision log cannot be marked implemented by this two-operation example: collection pagination, existing Angular feature extraction, complete SCSS/Tailwind migration, measured timeouts, financial mutation handoff, and export jobs require their own work and evidence. This file records the applicable demonstration without changing those decision statuses.

## Verification on 2026-11-10

- IdentityAccess API, migration runner, and AppHost built with zero warnings/errors when `NuGetAudit=false` was supplied for the build. An ordinary AppHost restore failed on an existing transitive MessagePack NU1902 advisory; no dependency was changed in this demo.
- Focused IdentityAccess unit tests passed 4/4; HTTP test-host integration tests passed 3/3 for `201` without `Location`, get-one, typed `400`/`409`/`404`, and generic non-disclosing `500`. Architecture tests passed 21/21 after including the new service ownership and operation contracts. Angular production build passed; Angular tests passed 12/12. Browser inspection confirmed the `/identity-demo` route can create a synthetic in-memory user and retrieve the same user by ID.
- Pinned EF tooling reported no model changes since the generated migration. Generated SQL contains one `users` table and unique indexes for `create_request_id` and `normalized_email`. `lg docs` and `git diff --check` passed.
- After adding the API's loopback launch profile, the non-Docker Aspire resource-model test passed 1/1.
- On 2026-11-11, the explicitly enabled Aspire distributed test passed 1/1 against ephemeral PostgreSQL with no persistent volumes. It applied the IdentityAccess migration, reached a healthy `identity-api`, created and fetched one synthetic user, returned the same user ID on an identical request-ID retry, and returned `409` for a reused request ID with changed input and a case-variant duplicate email. Test containers stopped afterward, and no named test volumes were found. That distributed test covered the backend directly before the Angular page was connected to it.

## Local Angular-to-database connection (2026-11-11)

The /identity-demo feature now calls the two IdentityAccess operations over same-origin /identity-demo-api URLs. Aspire injects the internal API endpoint into the Angular development server, whose proxy removes that prefix before forwarding. The API persists through its service-owned EF Core context to the ledgerlens_identity PostgreSQL database. This is a loopback-only development demonstration using synthetic data; the Gateway still has no IdentityAccess route. Authentication and non-loopback exposure remain separate gates.

On 2026-11-12, the Angular HTTP tests passed 2/2 and the production Angular build and AppHost build passed. Aspire started ephemeral PostgreSQL, ran the IdentityAccess migration, and served the Angular page through its local proxy. A synthetic user was created in the browser, the page was reloaded, and the same user was retrieved by ID. A separate HTTP smoke test also created and fetched a synthetic user through the Aspire-assigned Angular endpoint. The ephemeral PostgreSQL and NATS containers were stopped and removed after verification.
