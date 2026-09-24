# Identity, Access, Google OIDC, and UI Resource Foundation

Last reviewed: 2026-09-24. Status: Design-only proposal; implementation is not authorized and has not started.

## Goal

Replace the fixed local principal through the smallest secure, reversible vertical slices: Google OIDC at the Gateway BFF, revocable cookie sessions, LedgerLens-managed TOTP MFA and critical-operation step-up, independently verifiable internal service identity, unchanged financial owner isolation, user/role/resource authorization, a security/session dashboard, and English/Thai menu/page/action localization through shared resource keys.

This plan does not authorize non-loopback exposure. It follows [ADR-0019](../../adr/0019-google-oidc-bff-session.md), [ADR-0020](../../adr/0020-gateway-internal-service-tokens.md), [ADR-0021](../../adr/0021-resource-key-access-and-localization.md), the detailed [identity/access architecture](../../architecture/IDENTITY_ACCESS_AND_UI_LOCALIZATION.md), and the [session/MFA administration design](../../architecture/SESSION_AND_MFA_ADMINISTRATION.md). All three ADRs remain Proposed until the owner confirms the open decisions and the relevant evidence gates pass.

Authentication and multi-user authorization remain outside Version 1 and scheduled as optional Roadmap Phase 6. Before any slice begins, the owner must explicitly reprioritize the work and the same change must update product scope, roadmap ordering, ADR-0015's service roster, and the relevant proposed ADR statuses. Merely keeping this plan in `active/` does not grant implementation authority.

## Boundaries

- Google Sign-In is authentication only with `openid email profile`; no Gmail API, access token, refresh token, or additional provider.
- Email is profile data. Durable external identity is `(issuer, subject)`; durable LedgerLens identity is UUIDv7.
- Angular never persists an authentication token. Gateway remains the browser-facing BFF.
- Business services never trust browser identity headers or the Google ID token.
- Portfolio ownership predicates remain mandatory and are tested with two users.
- `ExternalIdentity` never becomes a delegated OAuth-token store.
- Existing `{ data, meta }`, Problem Details, API naming, explicit-handler, service-owned database, privacy, and synthetic-data rules remain in force.
- The accepted Angular feature-route restructure is a prerequisite/coordination point, not part of this plan's first backend slices.

## Slice 0 — decisions, research, and threat model

Confirm closed registration, IdentityAccess service ownership, session lifetimes, MFA policy, presence/activity semantics and retention, stable development origin, popup/button-only Google UX, administrator-managed UI text, and short-lived internal tokens. Recheck official documentation, candidate dependency versions/licenses/advisories/transitives, spike narrow ASP.NET Core Identity reuse versus a custom TOTP aggregate, and maintain the repository-grounded threat model.

**Current evidence:** [LedgerLens Identity, Access, and UI Resource Threat Model](../../../LedgerLens-threat-model.md) records the current/proposed boundaries, prioritized abuse paths, controls, and focus paths under explicit localhost/closed-registration/small-admin assumptions. Owner confirmation and production-host specifics remain open.

**Acceptance:** ADRs have owner-approved statuses; dependency inventory and risk register are current; trust boundaries, secrets, key rotation, account linking, fixed-owner migration, backup/restore, and rollback are decision-complete. No code or migration starts while an evidence gate is unresolved.

## Slice 1 — IdentityAccess foundation without Google

Add the IdentityAccess Domain/Application/Infrastructure/Database/Migrations/API projects, `identity-db`, AppHost resources, architecture tests, and reviewed migrations. Implement local users, external identities, sessions with assurance/security versions, selected MFA credential/recovery stores, roles, access resources, grants/overrides, UI texts, culture preference, and privacy-safe audit records with synthetic fixtures only.

**Acceptance:** UUIDv7, unique issuer/subject, transactional concurrent provisioning, status/session rules, resource precedence, culture fallback, versioning, audit, migration drift, PostgreSQL application, backups, and service boundaries pass. The service remains internal and does not change current browser behavior.

## Slice 2 — synthetic BFF session vertical slice

Add Gateway cookie authentication, antiforgery, exact Origin/Referer policy, rate limits, persistent external Data Protection keys, auth endpoints, and a development/test-only synthetic verifier. Implement session creation/introspection/revocation without Google network access.

**Acceptance:** anonymous/authenticated session bootstrap, cookie flags, idle/absolute expiry, renewal, logout/current/all-session revocation, CSRF, login CSRF nonce, rate limits, disabled user, key persistence/rotation overlap, and non-disclosure tests pass. Fixed-user business APIs remain available only through the explicit compatibility profile.

## Slice 3 — secure internal identity propagation

Add Gateway token issuance/YARP transforms and service JWT validation. Replace Portfolio Core's runtime `FixedLocalCurrentUser` with a claims-backed implementation behind the authenticated profile while retaining fixed synthetic identity for existing tests during migration. Add endpoint permission policies without removing owner predicates.

**Acceptance:** inbound identity headers/cookies/tokens are stripped, service audiences are isolated, wrong/expired/unsigned tokens receive `401`, missing action permission receives `403`, and User A cannot read or mutate User B's portfolio through direct backend or Gateway tests. No email/provider claim reaches business services.

## Slice 4 — fixed-owner migration rehearsal

Implement the one-time controlled link command and dry-run/reporting path. Back up synthetic data, link a verified synthetic issuer/subject to the exact existing fixed UUIDv7, switch authentication mode, verify all ownership counts/data, then rehearse rollback without rewriting Portfolio owners.

**Acceptance:** no email auto-link exists; conflicts fail closed; prior Portfolio rows keep their owner UUID; the migrated owner retains access; a second user has none; rollback restores the fixed compatibility profile before its eventual removal.

## Slice 5 — Google Identity Services

Admit the verified Google library/dependency, add external configuration, implement server verification, and add the Angular login feature through the accepted feature-first route structure. Use an explicit button/popup only; defer One Tap/automatic sign-in.

**Acceptance:** valid login succeeds; invalid signature, audience, issuer, expiry, nonce, and unverified email fail with localized safe `401`; unknown users follow closed-registration policy; repeat/concurrent login is idempotent; changed email keeps the same user; no Google credential/token reaches storage or logs; CSP/COOP/Referrer Policy and accessible loading/error flows pass in supported browsers.

## Slice 5A — LedgerLens MFA and critical-operation step-up

Implement the accepted TOTP/recovery/lockout design after the framework spike. Google remains the primary sign-in; required users receive only a short-lived restricted pre-authentication transaction until TOTP or a one-time recovery code succeeds. Rotate the session identifier after verification and require recent MFA for critical administrator/security operations.

**Acceptance:** required primary-only users cannot access business routes; enrollment proof activates a secret only after a valid code; TOTP clock-window/replay/rate-limit/lockout and atomic recovery-code tests pass with `TimeProvider`; MFA reset revokes all target sessions; session rotation prevents fixation; critical operations reject absent/stale step-up; no secret/recovery code/token appears in API responses, logs, traces, fixtures, or screenshots.

## Slice 6A — typed resource manifest and catalog synchronization

Add the versioned source manifest, deterministic typed Angular key generator, manifest validation/static architecture checks, IdentityAccess preview/apply handlers, and a local `identity-catalog-sync` Aspire resource with Preview and Apply commands. New resources are Draft/default-deny; removals become RetirementPending; renames require explicit migration.

**Acceptance:** duplicate/orphan/cycle/invalid/missing-language inputs fail; generated output is deterministic; unregistered raw permission strings fail repository checks; preview writes nothing; apply requires the exact preview hash, is transactional/idempotent, preserves administrator translations/grants/audit, and is unavailable outside Development/deployment identity; the pinned Aspire command API is verified.

## Slice 6B — complete user and role/group administration

Implement Administration routes/pages for users, user detail, roles/groups, members, hierarchical permission matrix, per-user overrides, sessions, resource catalog, translations, and access audit as specified in [Access Administration and UI Resource Catalog](../../architecture/ACCESS_ADMINISTRATION_AND_RESOURCE_CATALOG.md).

**Acceptance:** create/clone/update/archive role, add/remove user membership, enable/disable user, effective-access source, user allow/deny precedence, atomic versioned permission replacement, last-admin/self-lockout prevention, session revocation, bounded pagination/search, audit, accessibility, and direct server authorization tests pass.

## Slice 6C — menus, pages, buttons, and language at runtime

Activate the reviewed resource catalog and English/Thai defaults. Implement authorized navigation, atomic page bootstrap, permission/translation versioning, cache invalidation, and endpoint policies using the same action keys as Angular controls.

**Acceptance:** a role controls menu/page/action access; language switch reloads authorized content; a page never renders mixed permission/text versions; missing Thai falls back to English; admin changes invalidate bounded caches; hiding/disabling a button is backed by a server denial test; API error RESX behavior remains unchanged.

## Slice 6D — security dashboard and session administration

Implement `Administration > Security` and the User detail Security tab from [Session Visibility, Security Dashboard, and MFA Administration](../../architecture/SESSION_AND_MFA_ADMINISTRATION.md). Report active unrevoked sessions and separately label recently active users from coalesced activity; never claim exact online presence or display token values.

**Acceptance:** dashboard totals reconcile to session queries; effective idle/absolute expiry is correct at boundary instants; one/all-session revocation invalidates access within the documented bound; administrator session enumeration/revocation and MFA reset require exact permissions plus recent step-up; User A cannot enumerate User B; privacy, pagination, accessible state, English/Thai, deterministic-clock, and stale-cache tests pass.

## Slice 7 — local Aspire and distributed closeout

Provide stable `http://localhost:4200` interactive origin without sacrificing dynamic parallel distributed-test endpoints. Add IdentityAccess health/migrations, external key/config parameters, synthetic distributed identities, and complete graph coverage.

**Acceptance:** `lg run` supports the registered local Google origin; distributed tests remain isolated and contact no Google service; all resources are healthy; browser-to-Gateway-to-service identity/ownership flow passes; direct service/browser spoofing fails; build/test/audit/privacy/model-drift/credential scans and `git diff --check` pass.

## Slice 8 — remove fixed-user runtime compatibility

Remove fixed-user production/runtime parameters and implementation only after the owner migration, rollback, and complete gates pass. Retain dedicated synthetic test-auth facilities that cannot activate in deployed profiles.

**Acceptance:** ordinary runtime cannot start in fixed-user mode; no fixed user secret is required; all prior financial ownership remains; documentation and operational recovery procedures match verified behavior.

## External-access gate

Even after all slices pass locally, LAN/public deployment remains prohibited until HTTPS, exact origins/hosts, proxy headers, CSP/secure headers, production key/secret storage, encrypted backups, dashboard/service/database isolation, rate-limit testing, security review, threat-model closure, operational monitoring, and restore/revocation exercises are accepted for the target environment.

## Rollback/removal

- Before Slice 8, disable Google/session mode, revoke sessions, and re-enable the loopback-only fixed compatibility profile; Portfolio ownership IDs are unchanged.
- Google Identity Services can be removed without changing `IdentityUser`, roles, resource keys, or a future provider because external mapping is provider-neutral.
- Internal tokens can be replaced by a standards-based authorization server/token exchange while keeping service authorization policies and internal user IDs.
- Database UI translations can fall back to source-controlled English/Thai defaults; their failure must never grant a permission.
- IdentityAccess schema removal requires explicit data export/retention and is not coupled to deleting financial owners.

## Documentation updates during implementation

Every slice updates this plan and its relevant ADR/architecture/quality documentation. Update `PROJECT_STATE.md` only when verified behavior or project state changes. Keep proposed design and implemented evidence visibly distinct.
