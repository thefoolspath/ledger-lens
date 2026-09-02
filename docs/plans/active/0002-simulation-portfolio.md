# Manual Simulation Accounts

Last reviewed: 2026-09-02. Status: Implemented with PostgreSQL end-to-end and visual QA blocked by unavailable Docker.

## Goal

Let the local owner record hypothetical US-stock and ETF buys and sells, refresh market data on demand, and inspect what the position would be worth without changing any confirmed Investment Account, cash ledger, holding, lot, or reconciliation result. This manual scenario tracker is not the deferred Version 2 strategy bot or a broker connection.

## Decisions and boundaries

- Portfolio Core owns `SimulationAccount`, immutable `SimulationTradeEntry`, short-lived idempotent trade drafts, confirmed FIFO allocations, and valuation calculations in separate tables and aggregates.
- A simulation account has no funded cash ledger. Each buy introduces hypothetical capital; sells cannot exceed simulated holdings. Margin, shorts, options, dividends, corporate actions, benchmark comparison, scheduling, and broker execution are deferred.
- The user enters either quantity or gross security amount. Amount mode divides by the confirmed last-trade price, rounds quantity down to 12 decimal places, and reports the unused remainder. Fees and taxes are explicit non-negative USD assumptions outside gross.
- Market Data owns instruments, latest-trade snapshots, daily OHLCV candles, primary USD/THB FX, BOT benchmark FX, provider quotas, freshness, and provenance. Provider DTOs and keys remain backend-only.
- Angular orchestrates the two service calls through Gateway. Portfolio Core receives normalized market snapshots, validates their shape, and performs every authoritative calculation with `decimal`; JavaScript numbers are presentation-only.
- Apache ECharts renders an equity/cost-basis line view and an OHLCV candlestick view. Every chart has an accessible textual summary and table fallback.

## Vertical slices

### Slice 1 — provider-neutral market data

Implement symbol search, batch latest trade, daily candles, and USD/THB FX contracts with an explicitly enabled provider adapter. Keep a deterministic synthetic provider for automated tests. Twelve Data remains disabled until current local-display, cache, retention, and open-source terms are accepted; Alpaca remains a labelled fallback whose IEX/SIP entitlement is visible.

**Acceptance:** responses include instrument identity, provider/feed, price kind, as-of, retrieval time, freshness/delay, request ID, and retention-policy key; invalid symbols, quota failures, missing keys, stale data, and cancellation return Problem Details without leaking credentials.

**Current evidence:** Provider-neutral models, handlers, the four HTTP surfaces, an explicitly configured synthetic provider, and a terms/key-gated Twelve Data adapter are implemented. The focused Market Data unit test passed on 2026-09-01 (`1/1`). External Twelve Data behavior and terms remain unverified and therefore disabled by default outside Development.

### Slice 2 — isolated simulation ledger

Create simulation accounts and immutable buy/sell entries through preview/confirm drafts. Confirming the same draft is idempotent. Reversal/replacement corrections remain linked. Rebuild FIFO lots, average cost, realized P/L, remaining cost basis, and quantity deterministically.

**Acceptance:** multiple buys and partial sells reconcile exactly; overselling is rejected; amount and quantity modes obey scale rules; fees/taxes are included in cost/proceeds; no confirmed Portfolio Core account, cash entry, lot, holding, or balance changes.

**Current evidence:** Separate domain and persistence models, preview/confirm/correction handlers, HTTP endpoints, four-table EF migration, FIFO/average-cost rebuild, simple return, and valuation kernel are implemented. Portfolio Core builds with zero warnings/errors, unit tests pass (`10/10`), and `lg db check` reports no pending model changes. The distributed isolation scenario compiles but PostgreSQL execution remains blocked by the unavailable Docker daemon.

### Slice 3 — current and historical valuation

Accept normalized latest quote/FX snapshots and daily candle observations, calculate current USD/THB value, realized/unrealized P/L, return, stock/FX effects, and chart points, and persist immutable current valuation snapshots. Missing dependencies produce an explicit incomplete result rather than zero.

**Acceptance:** current and historical outputs are deterministic, source-labelled, and reconcile to the simulation ledger before display rounding. A delayed/EOD/market-closed snapshot remains confirmable only after its status is visible to the user.

The simple return denominator is cumulative active Buy cost (gross plus assumed fee/tax). Missing latest-trade observations produce explicit incomplete response rows and are not persisted as snapshots; missing current or acquisition FX keeps USD values complete while THB attribution remains incomplete.

### Slice 4 — Angular workflow and charts

Add separate Real and Simulation modes, a persistent simulation warning, symbol search, amount/quantity entry, price refresh, preview/confirm, correction, current valuation refresh, summary cards, accessible ECharts views, and responsive tables.

**Acceptance:** component and end-to-end tests complete a synthetic multi-buy/partial-sell flow, show provider/freshness metadata, render incomplete/error states, preserve keyboard and screen-reader access, and prove the real ledger is unchanged.

**Current evidence:** The separate Real/Simulation UI, persistent warning, symbol/quote/FX/candle workflow, preview/confirm/correction ticket, summary cards, explicit incomplete states, lazy ECharts value and OHLCV views, ARIA descriptions, and table fallbacks are implemented. The production build completes without warnings with a 259.42 kB raw initial bundle (68.40 kB estimated transfer); chart code is lazy. Angular tests pass (`4/4` across `2/2` files). End-to-end PostgreSQL and desktop/mobile browser evidence remain blocked by Docker.

## Verification summary — 2026-09-02

The complete repository build passes with zero warnings/errors. Independent non-Docker tests, EF model drift, diff whitespace, tracked privacy patterns, and production dependency audit pass. The explicitly enabled synthetic distributed flow includes idempotent confirmation, multi-buy/partial-sell, correction, valuation, and real-ledger isolation, and its project compiles; runtime execution stops before product code because Docker has no active daemon pipe. The feature is therefore implemented but not release-ready until that external gate and visual QA pass.

## Verification and documentation rule

Each slice updates its applicable Markdown before code and records current build/test/privacy/reconciliation evidence afterward. `PROJECT_STATE.md` changes from planned to implemented only after the corresponding behavior has current direct evidence. Tracked fixtures are newly created and synthetic; provider responses, secrets, and personal portfolio data never enter Git.
