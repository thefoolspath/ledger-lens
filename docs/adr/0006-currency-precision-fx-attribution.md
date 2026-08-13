# ADR-0006: Currency, Precision, and FX Attribution

## Status

Accepted by owner direction; precision scales remain validation-sensitive.

## Context

The owner measures the portfolio in USD but needs a THB view that separates security and currency effects.

## Decision

Use USD as portfolio base and THB as reporting currency. Store exact decimals and FX provenance. Separate actual exchange execution, transaction-date valuation, current valuation, and BOT benchmark rate roles.

Use the executed trade date for stock attribution and the settlement date for cash posting. Report stock effect at acquisition FX and stock-period FX effect on current value or net disposal proceeds. In parallel, track USD cash lots from actual exchanges and other USD inflows; allocate consumption by confirmed linkage, then auditable manual allocation, then deterministic FIFO within the account. Internal transfers carry basis, while an external USD deposit with unknown THB basis remains incomplete.

Expose a primary calculation using broker slip, manual, BOT, then provider precedence, together with a separate BOT benchmark column and their signed difference. The two columns are not additive. A non-overlapping bridge separates exchange execution, USD-cash FX, stock-price effect, stock FX, and exchange-back execution; a fully closed cycle reconciles exactly to actual THB received minus actual THB paid before rounding.

## Alternatives considered

THB base, current-rate-only translation, stock-only attribution, two independent overlapping reports, mandatory manual cash linkage, moving-average cash basis, fixed FIFO without correction, and binary floating point.

## Evidence

[Currency and FX](../architecture/CURRENCY_AND_FX.md) and [calculation research](../research/FINANCIAL_CALCULATION_AND_FX_EVALUATION.md).

## Consequences

Attribution is explainable and exact; each stock lot needs acquisition FX and each USD cash interval needs a carrying rate. Automatic FIFO is transparent and deterministic but remains an estimate of fungible cash usage. Dual primary/BOT columns expose provider differences without double counting them.

## Risks

Missing/non-trading-day rates, unknown external-USD basis, incorrect source linkage, differing provider definitions, confusing alternative columns as additive, and overprecision.

## Revisit conditions

Revisit scales, slip-rate meaning, fee treatment, and fallback policy after Dime samples and provider prototypes; retain the exact-reconciliation and versioned-correction invariants.
