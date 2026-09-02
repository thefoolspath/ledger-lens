# ADR-0017: Isolated Manual Simulation Accounts

## Status

Accepted by owner direction on 2026-09-01.

## Context

The owner wants to record a hypothetical purchase such as NVDA, refresh current prices, and see what the gain or loss would have been. Reusing confirmed Investment Accounts or ledger entries would allow hypothetical activity to contaminate auditable cash, holdings, lots, tax views, and reconciliation.

## Decision

Portfolio Core stores manual simulations in separate `SimulationAccount` and `SimulationTradeEntry` aggregates and tables. They reuse the same deterministic calculation policies as confirmed trades but never reuse confirmed ledger rows. A simulation has no funded cash account; each buy introduces hypothetical capital and a sell cannot exceed simulated holdings. Every page and response identifies the result as simulated and non-executed.

Market Data remains the owner of external instrument, quote, candle, and FX observations. Angular coordinates calls through Gateway, while Portfolio Core performs canonical decimal calculations from normalized provenance-bearing snapshots. This avoids cross-service database access and backend-to-backend request chains.

## Alternatives considered

An `IsSimulated` flag on confirmed accounts or ledger rows, ownership by the Research service, Gateway business orchestration, and a full funded paper-brokerage ledger.

## Consequences

Isolation is enforceable through schema and tests, and calculations can be compared without weakening the confirmed ledger. Separate persistence and client orchestration add contracts and UI states. Supplied market snapshots are treated as external evidence and remain visibly source-labelled.

## Risks

Users may mistake a last trade for an executable fill or a simulation for a prediction. Persistent labels, freshness, feed, explicit fee/tax assumptions, and no broker-order surface mitigate this risk.

## Revisit conditions

Revisit when the deferred Version 2 strategy bot, funded paper cash, automated schedules, or broker execution is explicitly scoped.
