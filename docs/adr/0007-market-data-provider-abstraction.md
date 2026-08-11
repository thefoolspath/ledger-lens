# ADR-0007: Market-data Provider Abstraction

## Status

Accepted.

## Context

Coverage, quota, entitlement, freshness, retention, and pricing differ across providers and change over time.

## Decision

Define provider-neutral application contracts for symbols, quotes, candles, FX, profiles, fundamentals, actions, news, and calendars. Include provenance and freshness in every response. Keep credentials and provider DTOs in infrastructure.

## Alternatives considered

Direct provider SDK use throughout the application and a single generic untyped endpoint.

## Evidence

[Market architecture](../architecture/MARKET_DATA.md) and [provider evaluation](../research/MARKET_DATA_PROVIDER_EVALUATION.md).

## Consequences

Providers can be replaced and compared; normalization and capability discovery add work.

## Risks

An overly broad lowest-common-denominator interface.

## Revisit conditions

Split capability interfaces when the first two adapters reveal materially different semantics.
