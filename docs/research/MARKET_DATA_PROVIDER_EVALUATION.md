# Market Data Provider Evaluation

Research date: 2026-08-11. Pricing and entitlements can change and must be rechecked before signup or implementation.

## Official-source comparison

| Provider | Relevant facts | Fit | Open concern |
| --- | --- | --- | --- |
| Twelve Data | [Pricing](https://twelvedata.com/pricing) lists a free Basic personal tier with 8 API credits/minute and 800/day; [API docs](https://twelvedata.com/docs/advanced) cover latest price, EOD, historical series, ETFs, and FX | Best single-provider prototype candidate | Confirm local display, cache, retention, and open-source-app terms |
| Alpaca | [Market Data API](https://docs.alpaca.markets/us/docs/about-market-data-api) offers US stocks/ETFs, C# SDK, free Basic IEX real-time, 15-minute-limited SIP history, 200 requests/minute, and history since 2016 | Strong US quote/candle fallback | Requires an Alpaca account; free live coverage is IEX rather than consolidated market |
| Finnhub | [Pricing](https://finnhub.io/pricing) lists a personal free tier at 60 calls/minute with US coverage and limited fundamentals/news | Useful comparison/fallback | Detailed retention/display rights and endpoint coverage require account-level confirmation |
| Alpha Vantage | [Premium page](https://www.alphavantage.co/premium/) states the standard free allowance is 25 requests/day and entitlement is required for real-time/delayed US data | Useful low-frequency fallback | Free quota is small for portfolio plus research use |
| Bank of Thailand | [Official API](https://portal.api.bot.or.th/portal/catalogue-products/exchange-rates-1/48ad6c158fe84fe45c6e10120033c217/docs) publishes THB/USD weighted-average interbank reference endpoints | Preferred historical/daily official FX reference | It is a daily reference, not a real-time tradable quote; API key and availability testing required |

## Recommendation

1. Use Bank of Thailand as the official daily USD/THB reference candidate.
2. Prototype Twelve Data for latest-available US stock/ETF and indicative FX data.
3. Keep Alpaca as a US market-data fallback and compare its IEX limitation explicitly.
4. Do not accept a provider ADR until cache/retention/display rights for a personal local open-source client are confirmed.
5. Use provider-neutral contracts and make data provenance/freshness mandatory.

## MVP request budget

Fetch only on user action. Coalesce duplicate requests, fetch holdings/watchlist in bounded batches where supported, cache daily candles, use conditional refresh windows, and expose quota errors. No WebSocket is needed for Version 1.

## License rule

Open-source code does not make vendor data redistributable. Never commit captured responses or screenshots without checking terms and account metadata. Keep credentials server-side and make each user supply their own entitled key.
