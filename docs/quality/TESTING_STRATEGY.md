# Testing Strategy

Status: Accepted foundation gates; business test layers activate with their milestones.

## Foundation layers

- **Architecture:** per-service Domain/Application/Database/Infrastructure/API direction; no cross-service internals or common domain project. A Database project has no service project references and may be referenced only by its owning Infrastructure and Migrations projects.
- **Distributed application:** start AppHost, wait for every named resource, resolve dynamic endpoints, exercise Gateway-to-service routes, and call a backend through the Angular development proxy. Automated distributed tests disable persistent volumes; a separate local stop/start check verifies named-volume credential continuity.
- **Gateway integration:** route mapping, path transforms, ProblemDetails, timeout/cancellation, Host/Origin policy, and unavailable backend behaviour.
- **Infrastructure connectivity:** each service reaches only its named PostgreSQL database and NATS; no `EnsureCreated`, cross-database access, or business tables.
- **Frontend component:** Angular connectivity shell calls Gateway only and renders healthy/degraded/error states accessibly.
- **Observability:** one request produces correlated Gateway/backend logs and a distributed trace in Aspire Dashboard.
- **Privacy/security:** loopback-only exposure, forbidden paths, secret/PII patterns, log redaction, and internal endpoint exposure checks.

The repository `lg test` wrapper enables `LEDGERLENS_RUN_DISTRIBUTED_TESTS=1`, so its .NET pass exercises the complete ephemeral Aspire graph instead of reporting the guarded distributed test as passed without running its body. Stop any interactive `lg run` first because the Angular development server uses its local port while the distributed test is active. A direct run of `LedgerLens.DistributedAppTests.csproj` must set the same environment variable explicitly.

## Messaging gates for future business events

Test transactional Outbox crash windows, duplicate and out-of-order delivery, Inbox idempotency, poison-message retry/dead-letter behaviour, schema-version compatibility, correlation/causation propagation, and safe replay. Never test or claim exactly-once delivery.

## Business layers after foundation

Unit/property/reconciliation tests cover exact money/quantity arithmetic, ledger transitions, lots, attribution, corrections, and deterministic replay. Contract, PostgreSQL integration, component, end-to-end, backup/restore, provider, and parser tests activate with the owning milestone and use only synthetic data.

The first Milestone 2 acceptance path now creates a UUIDv7 Portfolio and Investment Account through Gateway, posts a synthetic USD Deposit and Withdrawal, and reads the owner-filtered PostgreSQL projection back to verify the exact `100.50 - 25.00 = 75.50` cash balance. Unit tests separately cover signed entry arithmetic, scale rejection, and UUIDv7 enforcement.

No release with changed financial behaviour proceeds without unit, property, reconciliation, migration, and end-to-end coverage.

Manual simulation acceptance adds deterministic amount/quantity conversion, multiple buys, partial sells, FIFO/average basis, fee/tax assumptions, correction, duplicate confirmation, oversell, stale/missing quote and FX, provider quota/error, and USD/THB attribution cases. Isolation tests compare confirmed cash/entries/holdings before and after the complete scenario. Angular tests require persistent simulation labelling, freshness/provenance, keyboard access, chart lifecycle cleanup, ARIA description, and a tabular fallback. Provider and end-to-end fixtures are newly created and synthetic.

Hybrid DB-first changes add three mandatory migration checks: `lg db check` must report no pending model change after migration generation; the full migration chain must create an empty ephemeral PostgreSQL database; and the new migration must preserve synthetic data when applied from its immediate predecessor. A source-database history stamp additionally requires schema-equivalence evidence and is not automated by the repository tooling.

## Manual simulation verification — 2026-09-18

- **PASS:** complete repository build, zero .NET warnings/errors, Angular production build, EF model/snapshot drift check, `git diff --check`, production npm audit (zero reported vulnerabilities), and tracked forbidden-path/credential-pattern scans.
- **PASS:** Portfolio Core unit tests `10/10`, including amount rounding, multi-buy/partial-sell FIFO, fee/tax, USD/THB completeness, oversell, simple return, and reversal/replacement rebuild; Market Data unit test `1/1`; Angular tests `6/6` across three files, including permanent separation labelling, chart accessibility/lifecycle disposal, canonical route coverage, and display formatting that suppresses floating-point presentation artifacts.
- **PASS:** the synthetic distributed scenario uses the canonical plan-0003 routes and exercises symbol/quote/FX, duplicate draft confirmation, two buys, partial sell, simulation correction, current valuation, and an unchanged confirmed cash/entry projection. On 2026-09-18 both distributed tests passed in 51 seconds against healthy ephemeral PostgreSQL and NATS resources, including the complete Portfolio Core migration chain.
- **PASS:** desktop and 390-by-844 mobile browser QA verified responsive reflow without horizontal document overflow, persistent simulation and non-order labelling, visible 3-pixel keyboard focus, source/freshness metadata, chart marker/tooltip rendering, accessible table-fallback parity, readable formatted money/quantity/percentage output, and no browser console warnings or errors.

## Project QA skill

The repository-local `$ledgerlens-qa` skill under `.agents/skills/ledgerlens-qa/` defines the repeatable QA workflow for milestone scoping, wrapper-based build and test gates, conditional distributed/UI testing, evidence capture, status classification, and release recommendations. It supplements this strategy without changing any acceptance criterion; this document and the active implementation plan remain authoritative.
