# Session Visibility, Security Dashboard, and MFA Administration

Last reviewed: 2026-09-23. Status: Proposed; no dashboard, session tracking, or MFA behavior is implemented.

This document extends [Identity, Access Control, and UI Localization Architecture](IDENTITY_ACCESS_AND_UI_LOCALIZATION.md) and [Access Administration and UI Resource Catalog](ACCESS_ADMINISTRATION_AND_RESOURCE_CATALOG.md). It defines what administrators may observe and control without exposing reusable credentials, and how LedgerLens can require a genuine second factor after Google sign-in.

## Product language and boundaries

The dashboard must not claim that a user is "online now" from ordinary HTTP activity. It reports two distinct facts:

- **Active session:** a server-side `UserSession` is not revoked and neither its idle nor absolute expiry has passed.
- **Recently active:** the session's coalesced `LastSeenAt` is within the configured presence window; propose five minutes.

Exact live presence would require browser heartbeats or a persistent SignalR connection and creates additional writes, privacy meaning, and stale-disconnect behavior. It is deferred until there is a demonstrated product need.

The displayed expiry is the LedgerLens session expiry, not a Google token expiry. Google ID tokens are verified once and discarded. Gateway-to-service JWTs are approximately 60-second per-request credentials and are never listed as user login sessions. The UI shows `IdleExpiresAt`, `AbsoluteExpiresAt`, and the effective earlier expiry, but never a cookie, token, token hash, Google subject, or MFA secret.

## Administration information architecture

The protected `Administration > Security` area contains three views.

### Security dashboard

Summary cards:

- recently active users;
- active sessions;
- sessions expiring in the next 15 minutes;
- users with MFA enabled versus users subject to an MFA requirement;
- successful and failed sign-in attempts in the last 24 hours;
- sessions revoked in the last 24 hours.

The bounded, pageable user table shows display name, account status, roles, active-session count, last successful sign-in, most recent activity, earliest effective session expiry, MFA enrollment state, and whether policy requires MFA. It never returns raw security evidence.

Selecting a user opens the existing User detail page on its **Security** tab. This keeps identity, roles, sessions, and audit context together instead of creating a second user editor.

### Sessions

The session list supports filters for user, state (`Active`, `RecentlyActive`, `IdleExpired`, `AbsoluteExpired`, `Revoked`), assurance level, created range, and expiring-soon state. Each row contains:

- session identifier;
- privacy-reviewed device label and coarse browser/platform summary supplied or derived without fingerprinting;
- created, last-seen, idle-expiry, absolute-expiry, and revoked instants;
- current assurance level (`Primary` or `Mfa`), last MFA verification time, and revocation reason;
- whether it is the administrator's current session.

An administrator with the exact revoke permission may revoke one session or every session for one user. Bulk revocation requires a reason, optimistic concurrency, antiforgery proof, recent MFA step-up, and an immutable audit event. The current administrator cannot accidentally revoke the last viable administrator recovery path without an explicit high-risk confirmation.

### MFA administration

The MFA view shows policy coverage and enrollment state only: `NotEnrolled`, `Enrolled`, `RecoveryRequired`, or `Locked`. It does not reveal the TOTP secret, QR seed, recovery codes, passkey public-key material, or provider authentication details.

An administrator may require enrollment, clear a lockout, or start an MFA reset. Reset is a destructive security operation: require recent MFA step-up, a reason, explicit confirmation, revoke all of the target user's sessions, invalidate all recovery codes, remove the old TOTP credential, and record actor/target/correlation identifiers. The user enrolls a replacement credential themselves; an administrator never receives it.

## Session lifecycle and presence

Proposed `UserSession` fields:

| Field | Purpose |
| --- | --- |
| `Id`, `UserId` | UUIDv7 session and internal user identity |
| `CreatedAt`, `LastSeenAt` | creation and coalesced activity |
| `IdleExpiresAt`, `AbsoluteExpiresAt` | sliding and hard session boundaries |
| `RevokedAt`, `RevocationReasonCode`, `RevokedByUserId` | immediate invalidation and audit |
| `AssuranceLevel`, `MfaVerifiedAt` | primary-only versus successful second factor |
| `SecurityVersion` | invalidate sessions after user/MFA/security-sensitive changes |
| `DeviceLabel`, `ClientSummary` | optional privacy-reviewed user recognition; no stable fingerprint |

The Gateway updates `LastSeenAt` at most once per session per minute and advances idle expiry according to the accepted sliding-session policy. This avoids a database write on every request. Dashboard freshness therefore has a documented tolerance rather than pretending to be real-time.

Every authenticated request checks the session/security version through the bounded IdentityAccess cache. Revocation, user disable, MFA reset, and explicit permission deny invalidate the relevant cache entry immediately; APIs still reject expired internal tokens independently.

Retention of expired/revoked session metadata and sign-in-attempt records is an owner decision. The default design retains only the minimum period needed for security investigation, then deletes or aggregates records. Raw IP addresses and full user-agent strings are excluded unless a separate privacy decision demonstrates their necessity.

## MFA decision

The first supported MFA method is TOTP through a standards-compatible authenticator application. Google OIDC remains the primary external authentication event; after it succeeds, LedgerLens enforces its own second factor when policy requires it.

This choice is deliberate:

- Google can return an `amr` claim when specifically requested and enabled, but ordinary Google sign-in must not be assumed to satisfy the LedgerLens MFA policy.
- ASP.NET Core Identity supports TOTP MFA.
- .NET 10 ASP.NET Core Identity supports WebAuthn passkeys, but the built-in passkey feature treats them as a primary authentication method and explicitly does not provide a built-in second-factor flow. Passkey-based step-up therefore remains a separate research/ADR gate rather than being presented as completed 2FA support.

Before implementation, compare a narrow use of ASP.NET Core Identity's MFA stores/managers with a custom IdentityAccess TOTP aggregate. ADR-0019's earlier reason for excluding Identity must be reopened because MFA, recovery codes, and lockout now exist. Prefer the supported framework path if it can preserve the Google-only primary login, service boundary, UUIDv7 identity, and custom UI without importing unused local-password behavior.

### Policy

Support these configuration states:

- `Disabled`;
- `Optional`;
- `RequiredForAdministrators` — recommended initial production policy;
- `RequiredForAll`.

Role or user changes that newly require MFA do not silently grant an authenticated session full access. Existing primary-only sessions become restricted to session, logout, and MFA enrollment/verification routes until upgraded or revoked.

### Authentication and step-up flow

1. Gateway validates Google OIDC and IdentityAccess resolves the internal user.
2. IdentityAccess evaluates account status, MFA policy, enrollment, lockout, and security version.
3. If MFA is not required, it creates a `Primary` session. If required, it creates a short-lived restricted pre-authentication transaction, not a business-capable session.
4. The user submits a TOTP or one-time recovery code to a rate-limited, antiforgery-protected endpoint.
5. On success, rotate the cookie/session identifier, create or upgrade the server-side session to `Mfa`, record `MfaVerifiedAt`, and destroy the pre-authentication transaction. Rotation prevents session fixation.
6. Critical operations require `Mfa` plus a recent verification, proposed as ten minutes. If stale, the Gateway returns a stable step-up-required Problem Details code and Angular opens the verification flow.

Critical operations include role/grant/override changes, user disable, session revocation for another user, MFA reset, identity linking, resource publication/synchronization, and changes to administrator status.

### Enrollment, TOTP, and recovery

- Enrollment requires a recent Google authentication and, when replacing an existing method, recent MFA verification.
- The pending TOTP secret is encrypted at rest with a key distinct from session/signing keys and is not activated until the user proves one valid code.
- Verification is rate limited, uses a small accepted clock window, stores the last accepted timestep to prevent replay, and produces safe failure codes without revealing whether the code or account state was wrong.
- Recovery codes are generated once, displayed once, stored as slow hashes, consumed atomically, and never recoverable by administrators.
- Successful recovery-code use is high-risk: notify in the UI, audit it, and require replacement of depleted codes.
- Clock drift, lockout thresholds, attempt windows, recovery-code count, secret encryption/key rotation, and retention require deterministic tests using `TimeProvider`.

## Permissions and API surface

Add the following immutable resource keys to the typed manifest:

```text
administration.security-dashboard.view
administration.sessions.view
administration.sessions.revoke
administration.mfa.view
administration.mfa.reset
self.security.manage
```

`administration.sessions.revoke` does not imply `administration.sessions.view`; each endpoint checks its exact key. Self-service security endpoints authorize the current subject and never accept a user ID supplied by the browser.

Proposed Version 1 operations:

```text
GET  /api/access/v1/security-dashboard/get-one
GET  /api/access/v1/user-sessions/get-page
GET  /api/access/v1/users/sessions/get-list/{userId}
POST /api/access/v1/user-sessions/revoke/{sessionId}
POST /api/access/v1/users/sessions/revoke-list/{userId}
POST /api/access/v1/users/mfa/update-requirement/{userId}
POST /api/access/v1/users/mfa/reset/{userId}

GET  /auth/mfa/status
POST /auth/mfa/totp/begin-enrollment
POST /auth/mfa/totp/complete-enrollment
POST /auth/mfa/verify
POST /auth/mfa/recovery-codes/regenerate
POST /auth/mfa/disable
```

All writes require antiforgery and exact Origin/Referer checks. Administrative writes require expected versions, reason codes where relevant, and audit. List responses use bounded pagination and server-side filters.

## Acceptance evidence

- active/recently-active/expired/revoked state is deterministic at boundary instants;
- coalesced activity does not extend absolute expiry or resurrect a revoked session;
- dashboard counts reconcile with the pageable session query under a fixed database snapshot;
- no API/log/trace contains cookies, tokens, TOTP secrets, recovery codes, Google subjects, raw IP addresses, or full user agents;
- one-session and all-session revocation take effect through Gateway and direct-service tests within the documented invalidation bound;
- primary-only sessions cannot access business/admin routes when MFA is required;
- TOTP enrollment, verification, replay, clock window, rate limit, lockout, recovery code, reset, and cookie-rotation tests pass using a deterministic clock;
- every critical administration operation rejects absent or stale MFA step-up;
- User A cannot enumerate or revoke User B sessions without the exact administrative permission;
- keyboard, screen-reader status, mobile layout, empty/loading/error states, and Thai/English text are verified;
- no real identity, secret, IP address, or session artifact enters tracked tests or screenshots.

## Owner decisions before implementation

1. Confirm `RequiredForAdministrators` as the initial MFA policy.
2. Confirm five minutes for “recently active,” one-minute activity coalescing, and a ten-minute critical-action step-up window.
3. Confirm session/audit retention and whether coarse client summaries are useful enough to store.
4. After a spike, accept narrow ASP.NET Core Identity reuse or a custom TOTP implementation; do not decide from documentation alone.
5. Decide whether passkeys are wanted as a later primary sign-in method or whether a separately reviewed WebAuthn step-up design is worth its added protocol surface.

## Official evidence

- [ASP.NET Core .NET 10 passkey support and limitations](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/passkeys/?view=aspnetcore-10.0)
- [ASP.NET Core multifactor authentication and TOTP guidance](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/mfa?view=aspnetcore-10.0)
- [Google OpenID Connect claims, including optional `amr`](https://developers.google.com/identity/openid-connect/openid-connect)
