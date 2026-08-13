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

Calculate this attribution per lot twice: a primary column using the confirmed rate precedence and a BOT benchmark column using the official reference for each boundary date. For each displayed metric:

```text
rate_difference   = primary_rate - bot_rate
result_difference = primary_result - bot_result
```

The columns are alternative views and are not additive. A missing benchmark leaves the comparison incomplete without invalidating an otherwise complete primary calculation.

## USD cash and exchange attribution

Let `Q` be an allocated USD cash quantity, `Fe` the reference FX on an exchange date, `Ft` the transaction-date FX at the next economic event, `Ain` the proportional actual THB cost allocated to `Q`, and `Aout` the proportional actual THB proceeds allocated to `Q` when converting USD back. Proportional allocation uses exact decimals and assigns any permitted posting-boundary remainder deterministically.

For THB-to-USD exchange and the subsequent period as USD cash:

```text
exchange_in_effect_thb = Q * Fe - Ain
pre_trade_cash_fx_thb  = Q * (Ft - Fe)
```

When a purchase consumes the cash, close the cash interval at the executed purchase FX and begin the stock lot at that same rate. When a sale occurs, close the stock interval at the executed sale FX and create a new USD cash lot with that carrying rate.

For USD cash held after a sale or other USD inflow, let `Fc` be its carrying rate and `Fx` the reference FX on the conversion-back or valuation date:

```text
cash_holding_fx_thb     = Q * (Fx - Fc)
exchange_back_effect_thb = Aout - Q * Fx
```

For a fully closed cycle, exchange execution, cash-holding FX, stock effect, stock FX, and exchange-back execution must sum exactly to `actual_thb_received - actual_thb_paid` before display rounding. Net dividends open a cash lot at their receipt-date FX. Acquisition charges join stock cost; disposal charges reduce net proceeds; other USD fees and taxes consume cash lots at their event rate.

Cash-lot allocations use confirmed linkage, then manual allocation, then FIFO. Calculate only the allocated portion of a partially consumed lot. Versioned allocation corrections rebuild all affected projections.

## Performance

Phase 1 provides absolute P/L and since-inception totals. Phase 2 adds daily-valued TWR; MWR/XIRR is added only after external cash-flow completeness is verified. TWR and MWR must be labelled rather than compared as interchangeable measures.

## Required edge cases

Fractional shares, multiple buys, partial sells, multiple FX exchanges, explicit and FIFO cash allocation, trade/settlement date differences, same-timestamp events, fees in a second currency, missing FX or external-USD basis, manual FX/allocation correction, internal USD transfer, stock splits, full disposal, dividend withholding tax, negative cash balance, exchange-back execution, and price/FX staleness.

## Evidence

- [GIPS discussion of TWR and MWR](https://stage.gipsstandards.org/standards/gips-standards-for-firms/gips-standards-handbook-for-firms/)
- [IRS stock basis overview](https://www.irs.gov/faqs/capital-gains-losses-and-sale-of-home/stocks-options-splits-traders/stocks-options-splits-traders-1) (evidence only; LedgerLens does not provide tax advice)
