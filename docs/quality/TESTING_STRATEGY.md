# Testing Strategy

Status: Accepted foundation gates; business test layers activate with their milestones.

## Foundation layers

- **Architecture:** per-service Domain/Application/Infrastructure/API direction; no cross-service internals or common domain project.
- **Distributed application:** start AppHost, wait for resource readiness, resolve dynamic endpoints, and exercise Gateway-to-service routes.
- **Gateway integration:** route mapping, path transforms, ProblemDetails, timeout/cancellation, Host/Origin policy, and unavailable backend behaviour.
- **Infrastructure connectivity:** each service reaches only its named PostgreSQL database and NATS; no `EnsureCreated`, cross-database access, or business tables.
- **Frontend component:** Angular connectivity shell calls Gateway only and renders healthy/degraded/error states accessibly.
- **Observability:** one request produces correlated Gateway/backend logs and a distributed trace in Aspire Dashboard.
- **Privacy/security:** loopback-only exposure, forbidden paths, secret/PII patterns, log redaction, and internal endpoint exposure checks.

## Messaging gates for future business events

Test transactional Outbox crash windows, duplicate and out-of-order delivery, Inbox idempotency, poison-message retry/dead-letter behaviour, schema-version compatibility, correlation/causation propagation, and safe replay. Never test or claim exactly-once delivery.

## Business layers after foundation

Unit/property/reconciliation tests cover exact money/quantity arithmetic, ledger transitions, lots, attribution, corrections, and deterministic replay. Contract, PostgreSQL integration, component, end-to-end, backup/restore, provider, and parser tests activate with the owning milestone and use only synthetic data.

No release with changed financial behaviour proceeds without unit, property, reconciliation, migration, and end-to-end coverage.

## Project QA skill

The repository-local `$ledgerlens-qa` skill under `.agents/skills/ledgerlens-qa/` defines the repeatable QA workflow for milestone scoping, wrapper-based build and test gates, conditional distributed/UI testing, evidence capture, status classification, and release recommendations. It supplements this strategy without changing any acceptance criterion; this document and the active implementation plan remain authoritative.
