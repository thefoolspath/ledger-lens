# Product Scope

## Version 1

- One local operator, one or more portfolios and Dime investment accounts.
- US stocks and ETFs; USD transactions and THB reporting.
- Manual transactions first, followed by reviewed Dime slip import.
- Buy, sell, deposit, withdrawal, dividend, fee, tax, currency exchange, and correction entries.
- Auditable lots, FIFO and average-cost views, realized/unrealized P/L, and price/FX attribution.
- On-demand latest quotes, daily history, and USD/THB FX with freshness labels.
- Holdings, transaction history, linked slips, portfolio summary, watchlist, and research notes.
- Local PostgreSQL persistence, backup, restore, health, and privacy-safe observability.
- Manual simulation accounts for user-entered hypothetical buys and sells, on-demand valuation, and accessible value/candlestick charts. Simulations have no funded cash ledger and never alter confirmed records.

## Version 2 — simulated trading bot

- Add a paper-trading and historical backtesting mode that simulates a trading bot without submitting orders to a broker or using real money.
- Let the user define or select a versioned strategy, simulation period, starting capital, position-sizing rules, and risk limits.
- Calculate simulated realized and unrealized profit/loss using deterministic decimal arithmetic, including configurable fees, taxes, spread, slippage, and rejected or partially filled orders.
- Compare the simulated result with a documented benchmark and show the trade log, equity curve, drawdown, assumptions, market-data provenance, and reproducible run identifier.
- Keep simulated portfolios, orders, and ledger entries visibly separated from confirmed real portfolio records so simulation results cannot alter actual holdings or balances.
- Label every result as a simulation rather than a prediction or guaranteed return. Version 2 does not include live order execution, broker credentials, unattended real-money trading, or investment advice.

Version 2 automation is distinct from the Version 1 manual simulation account: it adds versioned strategies, starting capital, risk limits, automated fills, and backtesting only after the manual and confirmed calculation foundations are verified.

## Designed but deferred

Corporate-action corrections are designed in the model; automated split/merger ingestion is deferred. TWR is planned after daily valuations are reliable; MWR/XIRR follows once complete cash-flow history exists.

## Out of scope for Version 1

Thai securities, mutual funds, gold, automatic trading, tax filing, continuous background operation, alerts, Windows notifications, public hosting, LAN access, authentication, multi-user authorization, mobile apps, and local AI.
