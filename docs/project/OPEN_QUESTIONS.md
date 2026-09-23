# Open Questions

Last reviewed: 2026-09-23.

## Owner decisions

- Confirm MIT as the repository license before adding `LICENSE`.
- Provide representative Dime slips for private local inspection and describe known layout/date variants.
- Confirm whether the official Bank of Thailand daily reference or a broker-displayed rate should be the default when both exist.
- Confirm the owner machine profile used to accept performance budgets.
- Confirm the identity/access owner decisions: IdentityAccess service ownership, closed registration/controlled owner link, session lifetimes, database-managed UI text, Gateway-issued internal tokens, fixed `http://localhost:4200` explicit-button Google development flow, and the split between local/deployment manifest sync and in-application administration/review.
- Confirm the security dashboard/MFA decisions: `RequiredForAdministrators` initial policy, five-minute recently-active window, one-minute activity coalescing, ten-minute critical-action step-up, session/audit retention and client-summary privacy, and the result of the required narrow ASP.NET Core Identity-versus-custom-TOTP spike. Passkeys are not counted as built-in 2FA in the .NET 10 design.

## Research/implementation gates

- Obtain unambiguous provider rights for local display, caching, retention, screenshots, and open-source integration.
- Validate current GA patch versions and Aspire/Angular startup on Windows with Docker Desktop.
- Validate proposed decimal scales and rounding against Dime/provider samples.
- Define exact corporate-action rules before automation.
- Decide data retention for candles, news, fundamentals, OCR artifacts, and backups after measured storage growth and terms review.

These questions do not block documentation completion. Provider selection and Dime parser implementation remain gated.
