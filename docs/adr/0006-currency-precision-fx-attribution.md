# ADR-0006: Currency, Precision, and FX Attribution

## Status

Accepted by owner direction; precision scales remain validation-sensitive.

## Context

The owner measures the portfolio in USD but needs a THB view that separates security and currency effects.

## Decision

Use USD as portfolio base and THB as reporting currency. Store exact decimals and FX provenance. Use slip/manual/BOT historical precedence and an on-demand latest rate. Report stock effect at acquisition FX and FX effect on current USD value so both exactly equal total THB P/L.

## Alternatives considered

THB base, current-rate-only translation, three-part attribution, and binary floating point.

## Evidence

[Currency and FX](../architecture/CURRENCY_AND_FX.md) and [calculation research](../research/FINANCIAL_CALCULATION_AND_FX_EVALUATION.md).

## Consequences

Attribution is explainable and exact; each lot needs acquisition FX and the convention assigns interaction to FX.

## Risks

Missing/non-trading-day rates, differing provider definitions, and overprecision.

## Revisit conditions

Revisit scales and fallback policy after Dime samples and provider prototypes; retain the exact-reconciliation invariant.
