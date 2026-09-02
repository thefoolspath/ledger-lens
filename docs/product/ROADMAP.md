# Product Roadmap

Last reviewed: 2026-09-01.

## Phase 0 — distributed project foundation

Create the complete coarse-grained microservice solution before any business behaviour: Angular shell, ASP.NET Core/YARP Gateway, Portfolio Core, Market Data, Slip Import API/worker, Research, Operations, per-service Clean Architecture projects, one PostgreSQL server with a database per service, NATS JetStream, Aspire ServiceDefaults, Aspire Dashboard, Code First migration infrastructure, and architecture/distributed-app tests.

The phase is complete only when one documented command starts the complete graph, every resource is healthy, Gateway routes to every service through Aspire service discovery, each service can reach only its own database and NATS, Angular reaches services only through Gateway, and logs/traces/metrics appear in Aspire Dashboard. No `UserProfile`, Portfolio, Ledger, quote, slip, research, calculation, or other business table/logic is allowed in this phase.

## Phase 1 — portfolio core MVP

Implement the fixed local current-user boundary, `UserProfile`, required Portfolio ownership, manual ledger entry, lots, dual cost views, USD/THB reporting, basic dashboard, coordinated backup, and restore inside or around Portfolio Core without splitting a financial transaction across services.

## Phase 2 — market data and performance

Implement Market Data provider adapters, FX and quote retrieval, symbol search, historical candles, freshness, quotas, watchlists, daily snapshots, manual Simulation Accounts, and versioned integration events. Manual simulations reuse the verified lot/calculation policies but remain structurally isolated from confirmed records. Add TWR, then MWR/XIRR only after cash-flow completeness is verified.

## Phase 3 — Dime slip reader

Implement the Slip Import document store, embedded-text extraction, OCR fallback worker, template parser, confidence, duplicate detection, review UI, Outbox/Inbox integration, and idempotent posting into Portfolio Core.

## Phase 4 — research center

Implement Research profiles, statements, comparable metrics, news/events, technical indicators, stock comparison, thesis, and journal in the Research service.

## Phase 5 — optional local AI

Evidence-grounded summaries only after evaluation, source citation, numeric grounding, and privacy controls pass.

## Phase 6 — optional authenticated access

OIDC sign-in, OAuth where delegated API access is required, local issuer/subject mapping, portfolio authorization, HTTPS, CSRF design, rate limiting, audit logging, service-to-service identity, and encrypted backups. Only after this gate may any non-loopback access be considered.

## Version 2 / Phase 7 — simulated trading bot

Implement isolated paper-trading and historical backtesting after the Version 1 ledger, market-data provenance, calculation, and reconciliation foundations are verified. A simulation run uses a versioned strategy and reproducible inputs to generate simulated orders, fills, positions, and realized/unrealized profit or loss. Results include configurable fees, taxes, spread, slippage, partial or rejected fills, benchmark comparison, drawdown, and a complete trade log.

Simulation records must never mutate confirmed portfolio holdings or cash balances. The UI must identify them as simulated results rather than predictions or guaranteed returns. This phase does not connect broker credentials, submit live orders, operate unattended real-money trading, or provide investment advice.

Alerts and Windows notifications remain deferred until a future operating mode justifies continuous or scheduled execution.
