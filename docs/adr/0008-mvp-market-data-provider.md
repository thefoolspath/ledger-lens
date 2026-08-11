# ADR-0008: MVP Market-data Provider

## Status

Proposed; blocked on terms confirmation.

## Context

Version 1 needs on-demand US stock/ETF prices, daily candles, and USD/THB without a continuous process.

## Decision

Prototype Twelve Data as the primary equity/indicative-FX candidate, Bank of Thailand as official daily THB/USD reference, and Alpaca Basic as US quote/candle fallback. Do not commit to production use until local display, cache, retention, and open-source-app rights are confirmed.

## Alternatives considered

Finnhub, Alpha Vantage, Alpaca only, and no external provider.

## Evidence

[Provider evaluation](../research/MARKET_DATA_PROVIDER_EVALUATION.md).

## Consequences

The prototype covers the required shapes with low initial cost; multiple providers require provenance and reconciliation.

## Risks

Terms, quotas, prices, coverage, and feeds can change; free data may not represent the consolidated market.

## Revisit conditions

Accept or replace after written/current terms review and a measured request-budget prototype.
