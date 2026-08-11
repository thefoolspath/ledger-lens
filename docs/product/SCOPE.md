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

## Designed but deferred

Corporate-action corrections are designed in the model; automated split/merger ingestion is deferred. TWR is planned after daily valuations are reliable; MWR/XIRR follows once complete cash-flow history exists.

## Out of scope for Version 1

Thai securities, mutual funds, gold, automatic trading, tax filing, continuous background operation, alerts, Windows notifications, public hosting, LAN access, authentication, multi-user authorization, mobile apps, and local AI.
