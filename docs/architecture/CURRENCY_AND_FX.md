# Currency and FX

Status: Proposed.

## Policy

- Portfolio base currency: USD.
- Reporting currency: THB.
- Store monetary amount with currency; never infer currency from portfolio defaults.
- Store FX as THB per USD with source, source type, as-of, retrieved-at, and quality status.

## Acquisition-rate precedence

1. Rate printed on and confirmed from the source slip.
2. User-confirmed manual rate.
3. Bank of Thailand daily weighted-average interbank THB/USD reference for the applicable date.
4. Configured provider historical rate.

Non-trading-day fallback uses the latest prior published reference date and records that date; it never relabels the rate as the trade instant. A manual replacement creates a versioned correction and triggers projection recalculation.

## Current rate

The user presses **Fetch Latest**. The backend fetches the latest entitled value, stores its metadata, and returns freshness. The UI may say real-time only if provider entitlement and metadata prove it; otherwise use delayed, end-of-day, reference, or latest-available.

The Bank of Thailand API is the preferred official daily reference candidate. A market-data provider may supply a more recent indicative rate, but the UI must distinguish it from the official daily reference.

See [Investment Calculations](INVESTMENT_CALCULATIONS.md) for exact attribution.

## Evidence

- [Bank of Thailand exchange-rate API](https://portal.api.bot.or.th/portal/catalogue-products/exchange-rates-1/48ad6c158fe84fe45c6e10120033c217/docs)
- [Bank of Thailand daily rates](https://www.bot.or.th/en/statistics/exchange-rate.html)
