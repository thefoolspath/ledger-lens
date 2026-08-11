# Architecture Decision Records

Last reviewed: 2026-08-11.

An ADR becomes Accepted only when owner direction and supporting evidence are sufficient. `Needs Evidence` blocks the dependent implementation milestone.

| ADR | Status | Decision |
| --- | --- | --- |
| [0001](0001-aspire-local-orchestration.md) | Accepted | Aspire for local orchestration |
| [0002](0002-modular-monolith-boundaries.md) | Superseded | Former modular-monolith direction; superseded by ADR-0015 |
| [0003](0003-postgresql-persistence.md) | Accepted for Version 1 | PostgreSQL 18 and EF/Npgsql 10 baseline |
| [0004](0004-auditable-ledger-corrections.md) | Accepted | Immutable economic history and correction entries |
| [0005](0005-dual-cost-basis-views.md) | Accepted | FIFO and average cost as first-class report views |
| [0006](0006-currency-precision-fx-attribution.md) | Accepted | USD base, THB report, exact two-part attribution |
| [0007](0007-market-data-provider-abstraction.md) | Accepted | Provider-neutral application contracts |
| [0008](0008-mvp-market-data-provider.md) | Proposed | Twelve Data candidate, BOT FX reference, Alpaca fallback |
| [0009](0009-dime-slip-extraction.md) | Needs Evidence | Layered embedded-text/OCR/template pipeline |
| [0010](0010-runtime-data-outside-repository.md) | Accepted | All private runtime data outside repository |
| [0011](0011-public-repository-safety.md) | Accepted | Defence-in-depth repository controls |
| [0012](0012-defer-authentication-future-oidc.md) | Deferred | Loopback-only V1; OAuth/OIDC gate for future access |
| [0013](0013-defer-local-ai.md) | Deferred | No local AI dependency in MVP |
| [0014](0014-explicit-handlers-before-mediator-library.md) | Accepted for Version 1 | Vertical slices and explicit command/query handlers before a mediator dependency |
| [0015](0015-coarse-grained-microservices.md) | Accepted for Version 1 | Coarse-grained services, service-owned databases, HTTP/JSON and NATS JetStream |
