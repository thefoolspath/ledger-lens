# Security and Privacy Architecture

Status: Proposed; required before implementation.

## Version 1 controls

- Resolve one fixed local principal through `ICurrentUser`. Its stable identifier and owner-supplied email live in User Secrets or external runtime configuration, never in tracked source, settings, migrations, logs, or fixtures.
- Require `Portfolio.OwnerUserId` and enforce portfolio ownership in every application command/query even before authentication exists. This is a data boundary, not a substitute for authentication.
- Bind web, API, PostgreSQL, Aspire dashboard, and internal tools to loopback only.
- Enforce allowed hosts and exact allowed origins; do not use wildcard CORS.
- Prefer same-origin web/API delivery. Mutating API calls require JSON plus a custom request header and validated Origin/Referer so hostile pages cannot use simple cross-origin form requests.
- Reject unexpected content types, methods, oversized bodies, archive bombs, and malformed PDFs.
- Use parameterized data access, output encoding, content-security policy, secure headers, request limits, and bounded concurrency.
- Do not enable dev tunnels, `0.0.0.0`, LAN binding, port forwarding, public domains, or external hosting.

## Runtime data boundary

Use an OS application-data directory or named volume outside the repository for PostgreSQL data, documents, OCR artifacts, exports, backups, logs, caches, protection keys, and secrets. Apply least-privilege filesystem permissions and recommend OS account protection and full-disk encryption.

## Logging

Do not log file bodies, full OCR text, transaction request bodies, account/reference identifiers, portfolio values unless essential, authorization headers, connection strings, or URLs containing keys. Use structured allowlisted fields, redaction, local retention, and deletion controls.

## Future network-access gate

OIDC sign-in, OAuth where delegated API access is needed, unique issuer/subject-to-local-user mapping, portfolio authorization, HTTPS, CSRF design, secure cookies/tokens, rate limiting, audit logging, hardened secret management, and encrypted backups must all be accepted before non-loopback access. Email alone must never be treated as the durable external identity.

## Evidence

- [ASP.NET Core CORS guidance](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0)
- [GitHub secret scanning](https://docs.github.com/en/code-security/concepts/secret-security/about-alerts)
