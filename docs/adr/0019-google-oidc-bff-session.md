# ADR-0019: Google OIDC Through a Cookie-Based Gateway BFF

## Status

Proposed; owner confirmation and implementation evidence are pending.

## Context

LedgerLens currently uses one externally configured fixed local user and is loopback-only. The owner wants Google Sign-In as an authentication/authorization learning and portfolio feature, without Gmail access, Google API access, browser-stored LedgerLens tokens, or additional social providers in the first slice. The later requirement for LedgerLens-managed two-factor authentication, recovery codes, lockout, session inventory, and administrator security operations reopens the framework choice described below.

The existing Gateway is already the only browser-facing backend and the documented future BFF/OIDC boundary. Portfolio Core already uses an internal UUIDv7 owner and must not replace that identity with email or a provider identifier.

## Proposed decision

Use Google Identity Services for OpenID Connect authentication with only `openid email profile`. Angular receives a short-lived Google ID token in memory and posts it once to the Gateway. The Gateway validates Google signature, issuer, exact audience, expiry, nonce, and verified-email policy, resolves `(issuer, subject)` through a new internal IdentityAccess service, and establishes a revocable LedgerLens session with an ASP.NET Core protected HttpOnly cookie.

Use the narrower built-in cookie authentication stack for the Google-only session slice and do not store Google access or refresh tokens. Before the MFA slice, spike narrow ASP.NET Core Identity reuse against a custom IdentityAccess TOTP aggregate. ASP.NET Core Identity is no longer rejected outright because LedgerLens now requires MFA, recovery codes, lockout, and security-sensitive account UI; the accepted implementation must still preserve Google as the primary login, UUIDv7 identity, the IdentityAccess boundary, and no local password unless separately approved.

IdentityAccess owns local identity, external mappings, sessions, roles, grants, and minimal login audit. Portfolio Core retains its service-local `UserProfile` and ownership model. The first owner is linked explicitly to the existing fixed UUIDv7; email matching is prohibited.

Default registration is closed. Unknown Google identities are denied until an owner-controlled provisioning/linking flow is accepted.

The design and exact contracts are specified in [Identity, Access Control, and UI Localization Architecture](../architecture/IDENTITY_ACCESS_AND_UI_LOCALIZATION.md) and [Session Visibility, Security Dashboard, and MFA Administration](../architecture/SESSION_AND_MFA_ADMINISTRATION.md).

## Alternatives considered

- Angular-held LedgerLens JWT: rejected because it increases browser token exposure and complicates revocation for a browser-only application.
- Forwarding the Google ID token to every service: rejected because it has the wrong audience, leaks provider identity, and couples business services to Google.
- Full ASP.NET Core Identity UI/local-password adoption: rejected for the Google-only slice. Narrow reuse for TOTP/recovery/lockout is now an explicit spike and owner decision before the MFA slice.
- Gateway-owned identity tables: rejected because it makes the transport/BFF host a persistence owner and complicates independent migration/testing.
- Portfolio Core-owned external identities: rejected as the long-term cross-service authority; Portfolio Core retains only its local user projection.

## Consequences

The design adds one service/database and a session-introspection dependency, but keeps the browser tokenless after login, preserves internal UUIDv7 ownership, and leaves a clean separation between sign-in identity and future delegated OAuth grants.

ADR-0012 remains effective until this ADR is accepted and its security gates are implemented. Acceptance of this ADR will supersede only the authentication deferral; it will not by itself authorize non-loopback exposure.

## Evidence gate

- owner decisions in the architecture document are confirmed;
- current Google/Microsoft/Angular/Aspire guidance and candidate package licenses/advisories are rechecked;
- threat model, exact data model, key lifecycle, fixed-user migration rehearsal, and two-user isolation plan are accepted;
- implementation passes the slice acceptance criteria before the status becomes Accepted.
