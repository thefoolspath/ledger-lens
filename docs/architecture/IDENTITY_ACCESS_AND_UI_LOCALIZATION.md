# Identity, Access Control, and UI Localization Architecture

Last reviewed: 2026-09-23. Status: Proposed; no authentication or authorization behavior is implemented.

## Evidence labels

- **Repository fact** describes source or documentation verified in this checkout.
- **Official fact** is supported by the linked Google, Microsoft, Angular, or Aspire documentation.
- **Proposal** is the recommended LedgerLens design and is not current behavior.
- **Owner decision** must be confirmed before the dependent implementation slice starts.

## Current state

- **Repository fact:** LedgerLens targets .NET 10, Aspire 13.4.6, Angular 22.1.1, YARP 2.3.0, PostgreSQL, EF Core/Npgsql, and service-owned databases.
- **Repository fact:** the Angular development server is browser-facing, calls relative `/api` URLs, and uses `proxy.conf.js` to forward them to the Aspire-discovered Gateway.
- **Repository fact:** the Gateway is a stateless YARP host. It has no authentication middleware, authorization policy, identity store, or session store.
- **Repository fact:** `app.routes.ts` is empty and the root Angular component currently owns the implemented workflows. The accepted but unimplemented frontend restructure will introduce feature routes and a thin shell.
- **Repository fact:** Portfolio Core resolves a configured `FixedLocalCurrentUser` through `ICurrentUser`. Its UUIDv7 identifier and email come from AppHost parameters/User Secrets.
- **Repository fact:** Portfolio Core owns `UserProfile`, requires `Portfolio.OwnerUserId`, and applies owner predicates in its data access. This is an effective data-isolation boundary but not authentication.
- **Repository fact:** all browser-facing and infrastructure resources remain loopback-only. ADR-0012 defers authentication and prohibits non-loopback exposure until the complete security gate passes.
- **Repository fact:** API successes use `{ data, meta }`; failures use localized Problem Details. Existing RESX error localization remains separate from the UI-content catalog proposed here.

## Gap analysis

The fixed-user mechanism cannot distinguish browsers or people, prove possession of an external identity, expire or revoke sessions, return `401` for an anonymous caller, or return `403` for a caller who lacks an operation permission. It also cannot safely propagate an authenticated principal through YARP. A raw browser header such as `X-User-Id` would be forgeable, while forwarding a Google ID token would couple every service to Google and give the token the wrong internal audience.

The current `UserProfile` and owner-filtered queries are useful migration anchors. They prevent an identity implementation from requiring a rewrite of Portfolio ownership, provided the migrated Google identity maps to the existing internal UUIDv7 rather than creating a second owner.

The Angular application also lacks route guards, session bootstrap, a central auth store, feature-level page bootstrap, and a stable contract joining menu/page/action permission keys to localized UI text.

## Recommended topology

**Proposal:** retain Angular and Gateway as the only browser-facing boundary and add one coarse-grained internal `IdentityAccess` service with its own PostgreSQL database and migration project.

```text
Browser
  |  Google Identity Services credential (login only)
  |  LedgerLens HttpOnly cookie (after login)
  v
Angular --same origin--> Gateway / BFF
                         |-- Google ID-token verification
                         |-- cookie and antiforgery enforcement
                         |-- session introspection
                         |-- YARP strips browser credentials
                         |-- short-lived audience-bound internal token
                         v
                   IdentityAccess API ---- identity database
                         |
                         +---- Portfolio Core / Market Data / other APIs
                               validate the internal token themselves
                               and enforce endpoint policy plus data ownership
```

`IdentityAccess` owns sign-in mappings, local account status, sessions, roles, resource grants, user overrides, user culture preference, the UI resource catalog, localized UI text, and minimal security audit events. Portfolio Core continues to own its local `UserProfile` projection and financial ownership. Other services persist local user projections only when their domain needs them; no cross-service foreign keys are introduced.

This adds a service because the data is cross-cutting, security-sensitive, and not owned by Portfolio, Operations, or a transport-only Gateway. It avoids giving the Gateway direct access to another service's database and avoids turning Operations into an unrelated identity store.

## Google authentication flow

Google is used only as an OpenID Connect identity provider. The admitted scopes are `openid`, `email`, and `profile`. No Gmail API is enabled, and LedgerLens does not request or persist a Google access token or refresh token.

1. Angular calls anonymous `GET /auth/session`. The Gateway returns the anonymous/authenticated state and establishes the Angular-compatible antiforgery token when required.
2. Angular calls `GET /auth/google/options`. The Gateway returns the public web client ID and a one-time nonce while binding the nonce to a short-lived protected pre-authentication cookie or server record.
3. Angular loads Google Identity Services from `https://accounts.google.com/gsi/client`, initializes it once, and renders only the explicit Sign in with Google button in the first slice. One Tap and automatic sign-in remain deferred.
4. Google returns a signed ID token to the Angular callback. Angular holds it only long enough to send `POST /auth/google` over the same origin and never writes it to Web Storage, IndexedDB, a log, or application state that survives the request.
5. The Gateway requires JSON, an exact allowed Origin/Referer, antiforgery proof, the bound nonce, a bounded body, and the login rate-limit policy.
6. The Gateway verifies the Google signature and key rotation, issuer, exact audience, expiration, nonce, and `email_verified`. It admits both documented issuer spellings but normalizes the stored issuer to `https://accounts.google.com`.
7. The Gateway sends the verified, minimal identity assertion to an internal IdentityAccess operation. That operation resolves `(Issuer, Subject)` transactionally, rejects disabled/deleted users, and either returns the existing internal UUIDv7 user or provisions according to the configured registration policy.
8. IdentityAccess creates a revocable `UserSession`; the Gateway issues the LedgerLens authentication cookie containing only the minimum protected principal and session identifier.
9. Angular discards the Google credential and refreshes `GET /auth/session`.

**Official fact:** Google requires server-side verification of signature, audience, issuer, and expiry, and states that `sub`, not email, is the durable account identifier. Google Identity Services separates authentication from API authorization.

### Registration and account linking

**Proposal:** default to `Closed` registration. An unknown Google identity receives `403 common.registration.closed`; it is not silently granted access to financial data. Development and distributed tests use a synthetic test verifier and synthetic identities rather than real Google accounts.

First-login provisioning is transactional within IdentityAccess. A unique `(issuer, subject)` constraint is the concurrency authority. If two first-login requests race, one insert wins and the other reloads the committed mapping; no duplicate local user is created.

Email is normalized profile metadata and an audit snapshot only. A changed email with the same issuer/subject updates allowed profile fields but resolves to the same internal user. LedgerLens never auto-links accounts by email. Linking an additional provider requires an authenticated session, recent reauthentication, antiforgery proof, and a unique external identity that is not already linked.

The first owner migration is a controlled link operation that maps the verified Google issuer/subject to the exact existing fixed-user UUIDv7. It is never an email match. The operation must fail if that UUID does not exist, owns no expected data when ownership was expected, is disabled, or already has a conflicting identity.

## Logout and session lifecycle

`POST /auth/logout` requires authentication and antiforgery validation. It revokes the current server-side session, signs out the cookie scheme, expires the cookie, and returns `204`. Angular then calls `google.accounts.id.disableAutoSelect()` so Google does not immediately choose the same account again. LedgerLens does not call a Google token-revocation endpoint because it never stores a Google access or refresh token.

A separate, explicitly authorized operation can revoke all sessions for the current user. Password reset semantics are not present because LedgerLens does not own a password in this design.

Administrative session visibility, honest active-versus-recently-active semantics, session revocation, and LedgerLens-managed MFA/step-up are specified in [Session Visibility, Security Dashboard, and MFA Administration](SESSION_AND_MFA_ADMINISTRATION.md). The dashboard displays LedgerLens idle/absolute session expiry, never Google or internal JWT values. Google sign-in alone is not assumed to satisfy the application's MFA policy.

Proposed defaults, subject to owner confirmation:

- production cookie name `__Host-LedgerLens.Session`; development uses a clearly different loopback-only name;
- `HttpOnly=true`, `Path=/`, no `Domain`, `SameSite=Lax`;
- `Secure=true` outside the explicitly loopback-only HTTP development profile;
- 30-minute idle expiry with sliding renewal and an eight-hour absolute expiry;
- a UUIDv7 session identifier and a server-side revocation record;
- persistent Data Protection keys outside the repository, protected at rest and isolated by application/environment;
- no token, cookie value, Google subject, raw email, or picture URL in logs.

`SameSite=Lax` preserves a future standards-based redirect callback without choosing the cross-site `None` mode. If deployment separates Angular and Gateway origins, that is a new security decision rather than a CORS toggle.

## Identity and access data model

All LedgerLens-generated identifiers are UUIDv7. External issuer and subject values remain provider-native strings.

| Aggregate/table | Important fields and constraints |
| --- | --- |
| `identity_users` | `id`, `status`, `display_name`, normalized current email, email verification status, preferred culture, created/updated instants; email is not unique identity |
| `external_identities` | `id`, `user_id`, `issuer`, `subject`, email-at-link, created/last-login instants; unique `(issuer, subject)` and unique provider link per user where policy requires |
| `user_sessions` | `id`, `user_id`, issued/last-seen/idle-expiry/absolute-expiry/revoked instants, assurance level/MFA verification, security version, revocation metadata, optional privacy-reviewed device label/client summary |
| `mfa_credentials` | `id`, `user_id`, method/status, encrypted TOTP secret reference or future credential metadata, created/verified/last-used instants and version; never returned through administration APIs |
| `mfa_recovery_codes` | `id`, `user_id`, slow code hash, created/consumed instants; one-time use and never recoverable |
| `roles` | `id`, immutable key, localized display resource key, active/system flags |
| `user_roles` | `user_id`, `role_id`, grant/revoke metadata; unique pair |
| `access_resources` | immutable `key`, kind (`Menu`, `Page`, `Action`, `Capability`), optional parent, route, order, active flag |
| `role_resource_grants` | `role_id`, `resource_key`; unique pair; grants only |
| `user_resource_overrides` | `user_id`, `resource_key`, `Allow` or `Deny`, audit metadata; unique pair |
| `localized_resource_texts` | `resource_key`, culture, field (`label`, `title`, `description`, `tooltip`), value, version and audit metadata; unique composite key |
| `security_audit_events` | event type, user/session identifiers when known, success/failure code, time, correlation and trace identifiers; no credential or raw token |

Local deletion defaults to disable plus session revocation. Hard deletion requires a separate retention decision because financial records must retain stable ownership and audit meaning.

Google picture URLs are not persisted in the first slice. They are mutable third-party URLs, add tracking/referrer considerations, and are not needed for authorization. The session response can use initials or a local neutral avatar. Persisting or proxying pictures is a later privacy decision.

### Future OAuth grants

`ExternalIdentity` proves who signed in and never stores delegated API tokens. A future OAuth integration uses a separate `ExternalAuthorizationGrant` aggregate owned by the service that calls the external API. It records provider, granted scopes, expiry, revocation state, and an encrypted secret reference. Authorization Code with PKCE, consent, refresh-token protection, and provider-specific removal are researched separately. Removing a delegated grant must not delete the user's sign-in identity.

## Resource-key permission and localization contract

An immutable `ResourceKey` is the shared contract for navigation, page access, actionable controls, endpoint authorization, and the localized label for that resource.

Examples:

```text
portfolio                                      Menu
portfolio.overview                             Page/View permission
portfolio.investment-accounts.create           Action/Button permission
portfolio.cash-ledger.correct                  Action/Button permission
```

The same action key is used by the Angular button, the page bootstrap payload, the backend authorization policy, the role grant, and the localized button label. Display-only copy that is not a permission uses a child text key such as `portfolio.overview.balance.heading`; not every sentence becomes a capability.

Authorization is default-deny. Effective access uses this precedence:

1. inactive/disabled user denies everything;
2. explicit user deny;
3. explicit user allow;
4. any active role grant;
5. deny by default.

The menu response contains only allowed navigation nodes. A page requires its page/view key. Each mutating endpoint requires its action key and still enforces resource ownership inside the application/data boundary. Missing capability produces `403`; a resource owned by another user remains concealed according to the operation's existing not-found policy.

Localized UI content is database-backed because the requested administrator-managed catalog needs versioning, audit, fallback, cache invalidation, and authorization. Stable keys and initial English/Thai defaults remain source-controlled. Resolution uses `Accept-Language`, supports `en-US` and `th-TH`, falls back to English, then to the resource key while emitting a privacy-safe missing-text diagnostic. This catalog does not replace RESX-backed Problem Details.

After session bootstrap, Angular loads authorized navigation once:

```http
GET /api/access/v1/navigation/get-list
Accept-Language: th-TH
```

Before activating a feature route, Angular loads one atomic page contract:

```http
GET /api/access/v1/page-bootstrap/get-one/portfolio.overview
Accept-Language: th-TH
```

The `{ data, meta }` response contains `pageKey`, `culture`, `canAccess`, allowed action keys, resolved page texts, `permissionVersion`, and `translationVersion`. Combining permission and text prevents a partially rendered page from mixing versions. ETag/version caching may avoid a full payload on repeat navigation, but the server remains authoritative.

The complete user/role administration screens, permission matrix, manifest authoring rules, catalog lifecycle, synchronization transaction, and local Aspire Dashboard commands are specified in [Access Administration and UI Resource Catalog](ACCESS_ADMINISTRATION_AND_RESOURCE_CATALOG.md). The key definition originates in a source-controlled manifest and generates typed Angular constants; database grants and administrator translations remain runtime data. Catalog synchronization never scrapes arbitrary frontend strings or automatically activates a new permission.

## Gateway and internal identity propagation

The Gateway requires authentication for all business YARP routes through a fallback or per-route policy. Health, readiness, the bounded auth endpoints, and explicitly admitted public assets are narrow exceptions.

Gateway-to-IdentityAccess calls that occur before a user session exists, including verified-login resolution, use a separate short-lived service token with `sub=ledgerlens-gateway`, audience `ledgerlens-identity`, and an exact internal operation permission. Session introspection/revocation calls use the same service identity class. IdentityAccess never accepts the browser's Google credential or an unsigned claim set directly from a public route.

For every proxied business request, YARP removes inbound `Authorization`, `Cookie`, `X-User-Id`, `X-User-Email`, `X-Role`, and similar identity headers. It mints a short-lived internal access token with:

- issuer `https://ledgerlens.internal/gateway`;
- internal UUIDv7 `sub`;
- one exact service audience;
- `iat`, `nbf`, `exp`, `jti`, and session `sid`;
- only the permission claims needed by that service/audience;
- no email, Google subject, name, picture, or Google credential.

The token is signed with an asymmetric key held by the Gateway; APIs receive only the public verification key. The private key and Data Protection keys are separate, external runtime secrets. A proposed token lifetime is 60 seconds. Each API validates signature, issuer, audience, lifetime, and required claims with inbound claim mapping disabled, then builds its claims-backed `ICurrentUser`. Direct requests without a valid service-audience token return `401`.

Ownership checks remain in service Application/Infrastructure code. Gateway authentication and an action permission do not prove that a portfolio belongs to the caller.

### Alternatives

| Alternative | Decision | Reason |
| --- | --- | --- |
| Angular stores a LedgerLens JWT | Reject | increases token exposure and makes revocation/CSRF/XSS trade-offs worse for this browser-only app |
| Forward Google ID token | Reject | wrong internal audience, provider coupling, unnecessary PII, and no LedgerLens authorization/session semantics |
| Forward/shared authentication cookie | Reject | shares Data Protection trust and browser credential semantics across every service |
| Unsigned identity headers | Reject | forgeable by any caller that reaches a service |
| Custom signed headers | Reject | reinvents canonicalization, expiry, audience, and replay rules that JWT validation already supplies |
| Gateway-issued short internal JWT | Recommend for the closed LedgerLens system | smallest standard validation surface with service audience isolation and no browser token storage |
| OAuth token exchange/independent service authentication | Defer | stronger fit when LedgerLens has external clients or a standards-based authorization server; excessive for the first local BFF slice |

## Angular design

The accepted feature-first Angular restructure should land before or with the auth UI slice so auth is not added to the current monolithic root component.

```text
src/app/core/auth/
  auth.models.ts
  auth.service.ts
  auth.store.ts
  auth.guard.ts
  permission.guard.ts
  auth.interceptor.ts
  google-identity.service.ts
src/app/core/localization/
  page-bootstrap.service.ts
  resource-text.store.ts
src/app/features/auth/login/
src/app/layout/user-menu/
```

- `AuthStore` uses signals with `unknown`, `anonymous`, `authenticated`, and `failed` states.
- Application startup calls `/auth/session` before protected routes render.
- Functional route guards return a redirect/URL tree and never act as the security boundary.
- A functional HTTP interceptor handles `401` by clearing local session state and navigating to login; `403` navigates to an access-denied view without pretending the user is logged out.
- Same-origin relative API requests use the cookie automatically. If a future deployment becomes cross-origin, explicit credentials and exact CORS require a new accepted design.
- Angular's XSRF support uses a dedicated readable antiforgery cookie and header; the authentication cookie stays HttpOnly.
- Protected feature routes load the atomic page bootstrap before activation. Templates check action keys through a typed directive/helper rather than scattered string comparisons.
- No auth or Google credential is stored in `localStorage` or `sessionStorage`.
- Loading, anonymous, forbidden, provider-error, expired-session, and offline states are explicit and accessible.

The CSP allows only the exact Google Identity Services script/frame/connect/style origins documented by Google plus LedgerLens's own origin. COOP is tested with popup and FedCM behavior; it is not loosened globally without browser evidence. Referrer Policy defaults to `strict-origin-when-cross-origin` or stricter where Google compatibility permits.

## Aspire and local development

The current `WithHttpEndpoint(targetPort: 4200)` gives the browser-facing Aspire proxy a dynamic host port. Google Authorized JavaScript Origins require an exact registered origin. The implementation must provide one stable development origin, proposed as `http://localhost:4200`, while retaining dynamic ports for distributed tests.

The local interactive profile will run Angular on the fixed loopback origin without a conflicting Aspire proxy port, using the supported endpoint/proxy configuration verified during implementation. The test profile continues to allocate isolated dynamic endpoints and replaces Google verification with a synthetic verifier. Tests never contact Google or use a real client/account.

AppHost will add `identity-db`, IdentityAccess migration/API resources, external runtime key locations, and the required references. Only Angular and Gateway receive external loopback endpoints. IdentityAccess and every business API remain internal. No design in this document authorizes LAN/public exposure.

## Configuration and secret matrix

| Value | Location | Classification | Source control |
| --- | --- | --- | --- |
| Google web client ID | Gateway runtime config; returned to Angular by auth options endpoint | public identifier, environment-specific | placeholder/schema allowed; real value external by project policy |
| Google client secret | not required for GIS ID-token sign-in | absent | never add |
| Allowed Google issuers/audience policy | Gateway options | non-secret policy | defaults/documentation allowed; environment value external |
| Angular/Gateway allowed origins | Gateway/AppHost options | security-sensitive config, not a credential | safe local defaults allowed; deployed values external |
| Cookie name/lifetimes | Gateway options | policy | allowed |
| Data Protection key ring | external application-data path/managed store | secret key material | forbidden |
| Internal-token signing private key | external secret store | secret | forbidden |
| Internal-token public verification key | API runtime config or trusted mounted file | public integrity configuration | test key only in tests; production value external |
| PostgreSQL credentials | Aspire parameters/User Secrets/production secret store | secret | forbidden |
| Fixed-user migration target UUID/email | one-time controlled migration input | private/runtime data | forbidden |
| Future OAuth client secret/tokens | server-side secret store/encrypted grant store | secret | forbidden and not part of Google sign-in slice |

## Fixed-user migration

1. Back up and verify the Portfolio database and ownership counts.
2. Deploy the IdentityAccess schema while the application remains loopback-only.
3. Run a one-time owner-link command that accepts the existing fixed UUIDv7 and a freshly verified Google issuer/subject. Do not infer the link from email.
4. Create the IdentityAccess user with that exact UUID and attach the external identity in one transaction.
5. Sign in and verify the session subject equals the existing owner UUID.
6. Prove the owner can read every prior portfolio and a second synthetic user cannot read or mutate any of them.
7. Keep the fixed-user provider behind an explicit local/test compatibility switch for one release/slice. It must be impossible to enable in a non-loopback or production profile.
8. Remove the compatibility provider and its secrets only after rollback evidence and distributed tests pass.

The owner-link command reads the short-lived Google credential and bootstrap secret interactively from standard input, never from command-line arguments, validates the credential through the same verifier, requires loopback Development plus an explicitly enabled migration mode, and writes no token or email to console/history. The bootstrap secret expires after one successful link and is stored only in User Secrets/external configuration.

Rollback before the removal step disables Google auth, revokes sessions, and restores the fixed-user profile; no Portfolio row or owner ID is rewritten. After fixed-user removal, rollback requires the preserved verified database/application backup and documented key compatibility.

## Trust boundaries and concise threat model

| Threat/abuse path | Control and verification |
| --- | --- |
| Forged/modified Google token | official library/JWK validation of signature, issuer, exact audience, expiry, nonce; invalid cases return generic `401` |
| Login CSRF or credential replay | exact Origin/Referer, Angular antiforgery, one-time nonce bound before login, rate limit, short token lifetime |
| Browser forges internal identity | strip all inbound auth/identity headers; APIs accept only audience-bound signed internal tokens |
| XSS steals session | HttpOnly cookie, restrictive CSP, no browser token storage, Angular output encoding; XSS still requires prevention because it can act as the user |
| CSRF mutates financial state | antiforgery header/cookie validation on all unsafe methods plus exact origin policy and SameSite cookie |
| User reads another portfolio | endpoint capability plus unchanged owner predicate; two-user PostgreSQL/distributed tests |
| Stale permission after admin change | permission version, bounded cache, session revalidation/revocation; security-sensitive denies invalidate immediately |
| Duplicate first login | unique issuer/subject and transactional insert/reload behavior |
| Disabled user retains access | session introspection/security version and revocation; short internal token lifetime |
| Key theft or rotation failure | external protected key storage, public/private separation, overlap during rotation, no secrets in telemetry or Git |
| Open registration exposes financial app | closed registration default, controlled owner link/invitation, no email auto-link |
| Direct service access | internal-only endpoints plus mandatory internal bearer validation; no browser cookie accepted by services |
| Auth dependency unavailable | fail closed for new login/session validation; existing cache has a short documented maximum; never fall back to fixed user outside the compatibility profile |

A repository-grounded threat-model review is a mandatory pre-implementation and pre-external-access gate. This document is not that final review.

## Test strategy

### Unit and contract

- Google verifier adapter: valid, invalid signature, wrong audience, wrong issuer, expired, nonce mismatch, and unverified email.
- Resource-key syntax, default deny, role union, user allow/deny precedence, disabled user, parent menu derivation, culture fallback, and missing-key behavior.
- Internal token claim minimization, audience isolation, expiry, key rotation overlap, and header stripping.
- Cookie/session idle and absolute expiry, renewal, current/all-session revocation, and security-version invalidation.

### Integration with PostgreSQL/test hosts

- first login provisions once; repeat and concurrent first login do not duplicate users or external identities;
- changed email with the same issuer/subject resolves the same internal user;
- account linking cannot use email and cannot link one external identity twice;
- disabled/deleted local user cannot create or refresh a session;
- anonymous is `401`, authenticated without capability is `403`, and cross-owner reads/writes do not reveal data;
- CSRF, Origin/Referer, content type, body size, rate limit, cookie flags, logout, and session revocation;
- database-backed UI text fallback, audit, versioning, and cache invalidation;
- logs and traces contain no ID token, cookie, internal token, Google subject, raw email, or forbidden PII.

### Angular and browser

- startup/session states, explicit Google button callback, guard redirects, page bootstrap before render, menu filtering, button/action gating, language switch, `401` expiry, `403`, logout, keyboard/focus, CSP and popup/COOP behavior;
- assert no auth credential is written to Web Storage;
- prove frontend hiding is not required for server denial by issuing the forbidden HTTP operation directly.

### Aspire distributed

- fixed interactive origin works through `lg run` while parallel tests retain dynamic isolation;
- the complete synthetic flow is Browser/Web -> Gateway cookie -> internal audience token -> service -> owner-filtered PostgreSQL;
- direct API request, forged header, wrong-audience token, expired token, and User A/User B isolation fail as expected;
- existing fixed-user distributed coverage remains until the migration slice explicitly replaces it.

## Dependency evaluation

- ASP.NET Core cookie authentication, authorization, antiforgery, Data Protection, and rate limiting are built into the admitted platform and are preferred over another authentication framework.
- `Google.Apis.Auth` is the candidate server verifier because Google recommends a platform library instead of handwritten JWT verification. As checked on 2026-09-22, NuGet lists version 1.76.0, Apache-2.0, Google ownership, .NET 6+/netstandard compatibility, and approximately 536 KB package size. Its exact transitive graph, advisories, and lockfile impact must be rechecked immediately before admission.
- `Microsoft.AspNetCore.Authentication.JwtBearer` is the candidate internal-token validator and must match the repository's .NET 10 patch line. The implementation gate must recheck its exact version/license/advisories and whether the shared framework already satisfies each required API.
- The existing YARP package remains the proxy/transform boundary.
- No Angular auth package, Google wrapper package, IdentityServer product, Redis, MediatR, mapper, or policy framework is admitted by this design. Google Identity Services is loaded directly behind a narrow adapter.

## Risks and trade-offs

- A new IdentityAccess service and database add startup, migration, backup, and availability cost, but avoid cross-database access and unclear ownership.
- Gateway-issued internal tokens create a small closed-system security-token responsibility. Key lifecycle, strict claims, short lifetime, and validation tests are mandatory; a standards-based authorization server becomes preferable if external clients or independent service trust appear.
- Database-managed UI text is more operationally complex than compiled translation files. Source-controlled defaults, fallback, versioned publishing, and audit are required.
- A fixed browser origin reduces local parallelism for interactive runs. Distributed tests keep dynamic ports and never depend on the registered Google origin.
- Session introspection adds an internal dependency. A short cache can improve availability but creates a measured revocation-delay trade-off.
- Google availability affects new sign-in, not existing financial data. The application must fail closed without corrupting or hiding owned data.

## Owner decisions required before implementation

1. Accept adding the `IdentityAccess` service/database and superseding ADR-0012's deferral while keeping external exposure prohibited.
2. Confirm closed registration and a controlled first-owner link rather than automatic Google-user provisioning.
3. Confirm the proposed 30-minute idle/eight-hour absolute session limits.
4. Confirm that button/page text is administrator-manageable database content while API error messages remain RESX-backed.
5. Confirm Gateway-issued 60-second, asymmetric, audience-specific internal tokens as the first internal propagation mechanism.
6. Confirm the stable interactive development origin `http://localhost:4200` and Google popup/button-only first slice.
7. Confirm the proposed split between local/deployment manifest ingestion and the LedgerLens administration UI: Aspire commands preview/apply source definitions, while administrators review/publish resources and manage roles, users, and translations in the application.

## Official references

- [Google Identity Services setup, CSP, and COOP](https://developers.google.com/identity/gsi/web/guides/get-google-api-clientid)
- [Verify the Google ID token server-side](https://developers.google.com/identity/gsi/web/guides/verify-google-id-token)
- [Google OpenID Connect reference](https://developers.google.com/identity/openid-connect/reference)
- [ASP.NET Core cookie authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0)
- [ASP.NET Core antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0)
- [ASP.NET Core Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-10.0)
- [YARP authentication and authorization](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/authn-authz?view=aspnetcore-10.0)
- [ASP.NET Core JWT bearer validation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Angular route guards](https://angular.dev/guide/routing/route-guards)
- [Angular HTTP/XSRF security](https://angular.dev/best-practices/security)
- [Aspire inner-loop networking](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview)
- [Aspire custom resource commands](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/custom-resource-commands)
