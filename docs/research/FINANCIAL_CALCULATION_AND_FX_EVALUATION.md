# Financial Calculation and FX Evaluation

Research dates: 2026-08-11 and 2026-08-13.

## Facts

- PostgreSQL documents `numeric`/`decimal` as exact numeric types of selectable precision: [data types](https://www.postgresql.org/docs/current/datatype.htm).
- PostgreSQL stores timezone-aware instants internally in UTC and uses the IANA timezone database: [date/time types](https://www.postgresql.org/docs/current/datatype-datetime.html).
- GIPS describes TWR as removing the effect of external cash flows and MWR as reflecting timing and size of those cash flows: [GIPS handbook](https://stage.gipsstandards.org/standards/gips-standards-for-firms/gips-standards-handbook-for-firms/).
- US IRS material states that ordinary stock basis generally includes purchase costs and that average basis has restricted tax uses: [stock basis FAQ](https://www.irs.gov/faqs/capital-gains-losses-and-sale-of-home/stocks-options-splits-traders/stocks-options-splits-traders-1). This is context, not LedgerLens tax guidance.
- IAS 21 requires initial recognition of a foreign-currency transaction using the spot exchange rate on the transaction date and subsequent translation of foreign-currency monetary items using the closing rate: [IAS 21](https://www.ifrs.org/content/dam/ifrs/publications/pdf-standards/english/2021/issued/part-a/ias-21-the-effects-of-changes-in-foreign-exchange-rates.pdf?bypass=on). LedgerLens uses this as calculation-design evidence, not as a claim of IFRS-compliant financial reporting.
- The Bank of Thailand publishes a daily weighted-average interbank THB/USD reference and separately identifies commercial-bank customer rates: [daily exchange rates](https://www.bot.or.th/en/statistics/exchange-rate.html). The reference is a benchmark, not proof of the user's executed rate.

## Inference

The economic ledger and explicit lots must be the source of truth. FIFO and average cost can both be durable user-facing calculations without storing two contradictory ledgers. Sales need explicit lot allocation; a FIFO suggestion is deterministic and reviewable.

The selected two-part THB attribution is exact when the stock effect is valued at acquisition FX and the FX effect is applied to current USD market value. It intentionally assigns the price/FX interaction to the FX component.

Actual exchange execution and transaction-date stock valuation answer different questions. A non-overlapping bridge can preserve both by closing each USD-cash interval at the next economic event and opening the stock or new cash interval at that same boundary rate. Because USD cash is fungible, source evidence should override an allocation policy; deterministic FIFO is an auditable fallback rather than a factual claim about which dollars funded a trade.

## Recommendation

- Use decimal arithmetic end-to-end and define rounding only at posting/provider/display boundaries.
- Store raw confirmed inputs, source currency, exact source rate, and versioned corrections.
- Calculate P/L per lot before aggregation to handle multiple acquisition dates and partial sales.
- Track USD cash lots independently from stock lots; use confirmed linkage, then versioned manual allocation, then FIFO within an account.
- Show primary and BOT benchmark calculations in parallel with provenance and signed differences, but never add the alternative columns together.
- Require a fully closed exchange/trade cycle to reconcile to actual THB received minus actual THB paid before rounding.
- Label average cost as an analytical view and avoid tax claims.
- Add TWR only after daily valuations and cash-flow segmentation are dependable; add MWR/XIRR after cash-flow completeness tests.

## Required evidence before acceptance

Owner-reviewed synthetic fixtures for fractional shares, multi-lot partial sale, multiple exchange lots, evidence/manual/FIFO cash allocation, trade-versus-settlement dates, fees/taxes, dividends, internal USD transfers, exchange back to THB, rate overrides, non-trading-day FX, and corporate-action corrections; executable identity and closed-cycle reconciliation tests; and a review of Dime rate meaning, fees, rounding, and precision from local-only samples.
