# Risk Register

Last reviewed: 2026-09-08.

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
