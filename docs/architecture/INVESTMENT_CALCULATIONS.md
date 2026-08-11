# Investment Calculation Specification

Status: Proposed; formulas require executable test vectors before acceptance.

## Exactness and signs

All canonical inputs and outputs use decimal arithmetic. Quantities are positive in trade details; ledger direction controls debit/credit. Display rounding occurs only at the presentation boundary. Internal values retain declared scale and use midpoint-to-even unless a broker/exchange rule requires a documented alternative.

## Trade values

```text
gross = quantity * unit_price
buy_cash_out = gross + fees + taxes
sell_cash_in = gross - fees - taxes
```

Acquisition fees and taxes are allocated into lot cost. Disposal fees and taxes reduce realized proceeds. A correction reverses/replaces economic effect through linked entries.

## Lots and cost views

- Every buy creates a lot with exact remaining quantity and allocated USD cost.
- Every sell records explicit allocations. Default suggestion is FIFO; the confirmed allocation is authoritative.
- FIFO view sums costs of lots in acquisition order.
- Average-cost view is `total remaining allocated cost / remaining quantity` after every confirmed event.
- Both views are first-class reports from one ledger. Average cost is analytical for ordinary US shares and is not presented as tax advice; tax treatment is jurisdiction-specific.

## Realized and unrealized results

```text
realized_usd = net_disposal_proceeds - allocated_lot_cost
unrealized_usd = current_market_value - remaining_lot_cost
weight = holding_current_value / portfolio_current_value
```

No P/L is computed when a required price, FX rate, or lot allocation is missing; the result is incomplete, not zero.

## THB two-part attribution

For each remaining lot, let `C0` be allocated USD cost including acquisition charges, `V1` be current USD market value, `F0` be acquisition THB per USD, and `F1` be current THB per USD.

```text
stock_effect_thb = (V1 - C0) * F0
fx_effect_thb    = V1 * (F1 - F0)
total_thb_pl     = V1 * F1 - C0 * F0
```

The identity `stock_effect_thb + fx_effect_thb = total_thb_pl` must hold exactly before display rounding. This convention assigns the interaction effect to FX and must be labelled. Realized attribution uses net USD proceeds and the disposal FX with the same exact-reconciliation principle.

## Performance

Phase 1 provides absolute P/L and since-inception totals. Phase 2 adds daily-valued TWR; MWR/XIRR is added only after external cash-flow completeness is verified. TWR and MWR must be labelled rather than compared as interchangeable measures.

## Required edge cases

Fractional shares, multiple buys, partial sells, same-timestamp events, fees in a second currency, missing FX, manual FX correction, stock splits, full disposal, dividend withholding tax, negative cash balance, and price/FX staleness.

## Evidence

- [GIPS discussion of TWR and MWR](https://stage.gipsstandards.org/standards/gips-standards-for-firms/gips-standards-handbook-for-firms/)
- [IRS stock basis overview](https://www.irs.gov/faqs/capital-gains-losses-and-sale-of-home/stocks-options-splits-traders/stocks-options-splits-traders-1) (evidence only; LedgerLens does not provide tax advice)
