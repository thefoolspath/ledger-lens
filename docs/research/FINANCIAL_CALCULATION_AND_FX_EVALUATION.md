# Financial Calculation and FX Evaluation

Research date: 2026-08-11.

## Facts

- PostgreSQL documents `numeric`/`decimal` as exact numeric types of selectable precision: [data types](https://www.postgresql.org/docs/current/datatype.htm).
- PostgreSQL stores timezone-aware instants internally in UTC and uses the IANA timezone database: [date/time types](https://www.postgresql.org/docs/current/datatype-datetime.html).
- GIPS describes TWR as removing the effect of external cash flows and MWR as reflecting timing and size of those cash flows: [GIPS handbook](https://stage.gipsstandards.org/standards/gips-standards-for-firms/gips-standards-handbook-for-firms/).
- US IRS material states that ordinary stock basis generally includes purchase costs and that average basis has restricted tax uses: [stock basis FAQ](https://www.irs.gov/faqs/capital-gains-losses-and-sale-of-home/stocks-options-splits-traders/stocks-options-splits-traders-1). This is context, not LedgerLens tax guidance.

## Inference

The economic ledger and explicit lots must be the source of truth. FIFO and average cost can both be durable user-facing calculations without storing two contradictory ledgers. Sales need explicit lot allocation; a FIFO suggestion is deterministic and reviewable.

The selected two-part THB attribution is exact when the stock effect is valued at acquisition FX and the FX effect is applied to current USD market value. It intentionally assigns the price/FX interaction to the FX component.

## Recommendation

- Use decimal arithmetic end-to-end and define rounding only at posting/provider/display boundaries.
- Store raw confirmed inputs, source currency, exact source rate, and versioned corrections.
- Calculate P/L per lot before aggregation to handle multiple acquisition dates and partial sales.
- Label average cost as an analytical view and avoid tax claims.
- Add TWR only after daily valuations and cash-flow segmentation are dependable; add MWR/XIRR after cash-flow completeness tests.

## Required evidence before acceptance

Owner-reviewed synthetic fixtures for fractional shares, multi-lot partial sale, fees/taxes, dividends, rate overrides, non-trading-day FX, and corporate-action corrections; executable identity and reconciliation tests; and a review of Dime rounding/precision from local-only samples.
