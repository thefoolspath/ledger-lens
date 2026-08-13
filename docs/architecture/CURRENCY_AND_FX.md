# Currency and FX

Status: Accepted policy; implementation and executable calculation fixtures are pending.

## Policy

- Portfolio base currency: USD.
- Reporting currency: THB.
- Store monetary amount with currency; never infer currency from portfolio defaults.
- Store FX as THB per USD with source, source type, as-of, retrieved-at, and quality status.
- Keep actual exchange execution, transaction-date valuation, current valuation, and benchmark rates as distinct roles. A rate for one role never silently substitutes for another.
- Provide both a primary attribution and a Bank of Thailand benchmark attribution. They are alternative views of the same result and must never be added together.

## Rate roles and precedence

For the **primary transaction-date valuation rate**, use:

1. Rate or THB valuation printed on and confirmed from the applicable broker trade slip.
2. User-confirmed manual rate.
3. Bank of Thailand daily weighted-average interbank THB/USD reference for the applicable date.
4. Configured provider historical rate.

The **BOT benchmark rate** is calculated separately from the official daily reference for the same applicable date. The UI shows the source and as-of date for both columns and calculates `primary - BOT`. A zero difference is displayed as `0`; a non-zero difference is displayed in red with a signed value and a non-colour indicator.

The **actual execution rate** for a currency exchange is derived from confirmed source and destination amounts. Preserve any quoted rate and explicit fee separately; the effective rate includes the actual cash effect. A BOT or provider rate is a benchmark and must not be presented as the rate actually received.

Non-trading-day fallback uses the latest prior published reference date and records that date; it never relabels the rate as the trade instant. A manual replacement creates a versioned correction and triggers projection recalculation.

For a stock purchase or sale, the applicable valuation date is the exchange-local executed trade date, not the settlement date. Preserve both timestamps: the trade projection uses execution time while the cash ledger posts at settlement. A pending receivable or payable bridges the interval when they differ.

## Current rate

The user presses **Fetch Latest**. The backend fetches the latest entitled value, stores its metadata, and returns freshness. The UI may say real-time only if provider entitlement and metadata prove it; otherwise use delayed, end-of-day, reference, or latest-available.

The Bank of Thailand API is the preferred official daily reference candidate. A market-data provider may supply a more recent indicative rate, but the UI must distinguish it from the official daily reference.

## USD cash-lot allocation

Every confirmed THB-to-USD exchange creates a USD cash lot containing the actual THB paid, USD received, fees, effective rate, opening reference rate, source, and provenance. Sale proceeds and net dividends create new USD cash lots at the applicable transaction-date valuation rate so the stock or income period closes before the subsequent USD-cash period begins.

Allocate USD consumption within an investment account in this order:

1. Confirmed source linkage from a slip or internal transfer.
2. User-confirmed manual allocation.
3. FIFO by cash-lot availability time, with a stable identifier as the final tie-breaker.

An internal USD transfer carries the allocated basis and provenance to the destination account. Unrelated accounts are never pooled. An external USD deposit without a confirmed THB basis remains explicitly incomplete. Manual reallocation is a versioned correction; it never erases the previous allocation.

The UI labels automatic allocations as `Estimated — FIFO`. This is a deterministic attribution policy, not a claim that the broker used those specific dollars.

## Non-overlapping THB bridge

The reporting bridge separates currency-exchange execution, USD cash holding, stock-price movement, and FX movement while held as stock. It resets the attribution boundary at each economic event so no interval is counted twice:

```text
exchange-in execution effect
+ USD-cash FX before purchase
+ stock-price effect
+ stock FX from purchase to sale/current valuation
+ USD-cash FX after sale
+ exchange-back execution effect
= total THB result
```

For a fully closed cycle, the sum must equal actual THB received minus actual THB paid before display rounding. For open positions, current stock and USD-cash values remain unrealized and use the selected current rate. Missing rates, basis, or allocations produce an incomplete result, never zero.

See [Investment Calculations](INVESTMENT_CALCULATIONS.md) for exact attribution.

## Evidence

- [Bank of Thailand exchange-rate API](https://portal.api.bot.or.th/portal/catalogue-products/exchange-rates-1/48ad6c158fe84fe45c6e10120033c217/docs)
- [Bank of Thailand daily rates](https://www.bot.or.th/en/statistics/exchange-rate.html)
