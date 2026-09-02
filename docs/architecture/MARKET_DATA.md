# Market Data Architecture

Status: Provider-neutral contracts and synthetic Development/test implementation are complete; live-provider admission remains conditional on terms confirmation.

## Provider-independent contracts

```text
SymbolSearch(query, market)
GetLatestQuote(instrument)
GetCandles(instrument, interval, range)
GetFxRate(pair, asOfPolicy)
GetCompanyProfile(instrument)
GetFundamentals(instrument, period)
GetCorporateActions(instrument, range)
GetNews(instruments, range)
GetMarketCalendar(market, range)
```

Every response wraps data with provider, entitlement/feed, data-as-of, retrieved-at, freshness class, delay when known, request ID when available, and license/retention policy key.

## Version 1 behaviour

- User-triggered fetch; no continuous WebSocket or always-running schedule.
- Daily candles and latest-available quote for held, watched, or explicitly searched instruments.
- Cache keys include provider/feed/instrument/interval/as-of; stale values remain visible with status.
- Retries use bounded exponential backoff with jitter for transient failures; quota and entitlement failures do not retry blindly.
- Backend owns keys. Angular receives only normalized data and metadata.
- Derived indicators are calculated locally from licensed retained candles.
- Manual simulations use the latest available `LastTrade`, not bid/ask, and never label that observation as an executable fill. Market-closed, delayed, and EOD values remain usable only with visible freshness and user confirmation.
- Normalized quote, candle, and FX snapshots supplied to Portfolio Core carry instrument identity, provider/feed, price kind, as-of, retrieved-at, freshness/delay, request ID, and retention-policy key. Angular never receives provider credentials or raw provider payloads.

## Manual simulation HTTP surface

```text
GET  /v1/instruments/search
POST /v1/quotes/latest
GET  /v1/instruments/{instrumentId}/candles
POST /v1/fx/latest
```

The first implementation keeps a deterministic synthetic provider for tests. An external adapter must fail closed when its key or explicit terms-acceptance setting is absent; the application does not silently scrape or switch feeds.

## Provider direction

Twelve Data is the leading MVP candidate because one API covers US equities/ETFs, historical series, latest price, and FX with a usable personal tier. Selection remains Proposed until personal local-display, cache, retention, and open-source use are confirmed in writing or unambiguously in current terms. Alpaca Basic is a fallback for US stocks/ETFs but its free real-time feed is IEX-only; Finnhub and Alpha Vantage remain comparison candidates.

See [provider research](../research/MARKET_DATA_PROVIDER_EVALUATION.md).
