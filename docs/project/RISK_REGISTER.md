# Risk Register

Last reviewed: 2026-09-23.

## R-FOUNDATION-ANGULAR-CLI-ADVISORY

- Status: Open; development-only exposure constrained to loopback
- Evidence: `npm audit` on 2026-08-11 reports a moderate transitive `@hono/node-server` advisory through Angular CLI 22.1.3; npm offers only an unsafe Angular major downgrade, not a compatible Angular 22 fix.
- Control: do not expose the Angular development server beyond loopback, do not enable the affected MCP/static-server capability, and re-run the audit when Angular CLI publishes a compatible fix.
- Runtime impact: none in the production browser bundle because Angular CLI and the affected chain are development dependencies.

| ID | Risk | Likelihood | Impact | Mitigation / trigger | Status |
| --- | --- | --- | --- | --- | --- |
| R-001 | Real portfolio data or secret enters public Git | Medium | Critical | External runtime boundary, scanners, forbidden paths; any detection stops publication | Open |
| R-002 | Unauthenticated localhost API is called by hostile origin | Medium | Critical | Loopback, Host/Origin/content-type controls and browser tests | Open |
| R-003 | Incorrect stock-lot, USD-cash-lot, P/L, or FX result | Medium | Critical | Exact arithmetic, versioned allocation corrections, reviewed fixtures, property tests, and closed-cycle reconciliation | Open |
| R-004 | Average cost is mistaken for tax basis | Medium | High | Analytical label and no tax claims | Open |
| R-005 | Provider terms prohibit desired display/cache | High | High | Terms gate, own-key model, swappable providers | Open |
| R-006 | Free feed is incomplete or stale | High | Medium | Provider/feed/freshness display and no real-time claims | Open |
| R-007 | Missing historical FX or external-USD basis changes attribution | Medium | High | Rate-role separation, provenance, explicit incomplete state, prior-day label, evidence/manual/FIFO allocation, and versioned correction | Open |
| R-008 | OCR misposts a financial transaction | Medium | Critical | Mandatory review, strict validation, no auto-post | Open |
| R-009 | Real Dime sample leaks into fixtures/logs | Medium | Critical | Private inspection and newly created synthetic fixture policy | Open |
| R-010 | Malformed document exhausts local resources | Medium | High | File/page/time/memory limits and process isolation gate | Open |
| R-011 | Database and slips restore inconsistently | Medium | High | Coordinated manifest/checksum and clean restore test | Open |
| R-012 | Preview or mismatched stack is implemented | Medium | Medium | Supported GA baseline and implementation-time patch check | Open |
| R-013 | Microservice foundation creates excessive local resource and coordination cost | Medium | High | Coarse boundaries, complete foundation benchmark, no service-per-CRUD, no distributed financial transaction | Open |
| R-014 | Documentation drifts from implementation | Medium | High | PROJECT_STATE updates and source-of-truth hierarchy | Open |
| R-015 | Disk usage grows through candles, OCR, or backups | Medium | Medium | Retention, quotas, metrics, and storage budgets | Open |
| R-016 | Corporate action invalidates lots | Medium | High | Explicit adjustment model, manual review; automation deferred | Open |
| R-017 | Simulated activity contaminates confirmed accounting | Low | Critical | Separate aggregates/tables, no shared discriminator; synthetic PostgreSQL scenario verifies confirmed cash and entries remain unchanged | Open |
| R-018 | Last trade or simulated P/L is mistaken for an executable result | Medium | High | Persistent simulation label, price-kind/feed/freshness display, explicit assumptions, no broker surface | Open |
| R-019 | Chart presentation diverges from authoritative decimal results | Medium | High | Backend decimal projections, display-only JavaScript numbers, accessible table parity tests | Open |
| R-020 | Google/session authentication maps to the wrong internal owner or auto-links by email | Low | Critical | Unique issuer/subject, closed registration, controlled exact-UUID owner link, no email linking, transactional concurrency and two-user isolation tests | Proposed control; implementation not started |
| R-021 | Browser or direct caller forges identity/permission at an internal API | Medium | Critical | Strip inbound identity credentials, audience-bound signed internal tokens, service validation, endpoint policies and unchanged owner predicates | Proposed control; implementation not started |
| R-022 | Session/signing/Data Protection keys leak or rotation invalidates access unexpectedly | Medium | Critical | External protected key storage, private/public separation, overlap/revocation tests, backup/recovery and log/Git scans | Proposed control; implementation not started |
| R-023 | Database-managed permission or translation cache serves stale/incorrect access | Medium | High | Default deny, explicit precedence, versioned publish, immediate deny invalidation, bounded cache, English fallback and audit | Proposed control; implementation not started |
| R-024 | Resource-catalog sync imports a typo/malicious key, drops grants, or grants access implicitly | Medium | Critical | Versioned manifest, typed generation, static raw-key checks, preview hash, transactional idempotent apply, Draft/default-deny additions, RetirementPending removals, explicit rename migration and audit | Proposed control; implementation not started |
| R-025 | MFA enrollment, reset, recovery, or step-up can be bypassed or used for account takeover | Medium | Critical | Restricted pre-auth transaction, TOTP proof before activation, rate limit/lockout/replay checks, slow-hashed one-time recovery codes, recent-MFA reset, cookie rotation, all-session revocation, deterministic tests and audit | Proposed control; implementation not started |
| R-026 | Security dashboard exposes credentials/PII or incorrectly reports a user as online | Medium | High | Show only server-side session metadata, separate active from recently active, coalesce `LastSeenAt`, exclude tokens/secrets/raw IP/full user agent, exact view permissions, bounded retention and privacy tests | Proposed control; implementation not started |
