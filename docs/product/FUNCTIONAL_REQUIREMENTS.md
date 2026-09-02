# Functional Requirements

| ID | Requirement | Initial acceptance |
| --- | --- | --- |
| FR-001 | Manage portfolios and investment accounts without hard-coding Dime into the domain | A portfolio owns one or more broker accounts |
| FR-002 | Record auditable financial events | Original entries remain visible after correction |
| FR-003 | Support fractional US stock and ETF quantities | Exact decimal quantities reconcile through partial sales |
| FR-004 | Provide FIFO and average-cost report views | Both derive from the same confirmed ledger |
| FR-005 | Calculate realized/unrealized P/L, fees, taxes, cash flow, value, and weight | Published synthetic examples match the engine |
| FR-006 | Report in USD and THB with non-overlapping stock, USD-cash, and exchange attribution | Primary and BOT benchmark columns show source and signed difference; each column reconciles independently, and a closed cycle equals actual THB received minus actual THB paid |
| FR-007 | Capture historical and latest FX with provenance | Source, as-of, retrieval time, and override history are visible |
| FR-008 | Fetch market and FX data on demand | No always-running scheduler is required |
| FR-009 | Label data freshness | Real-time, delayed, end-of-day, or latest-available is never ambiguous |
| FR-010 | Store and process Dime slips locally | Original, hash, extraction, review, and posted transaction remain linked |
| FR-011 | Prevent duplicate slip posting | Content hash and broker reference produce an explainable result |
| FR-012 | Require human confirmation before posting extraction | Low-confidence or missing fields cannot auto-post |
| FR-013 | Provide dashboard, holdings, ledger, lots, and slip inbox views | Each view is traceable to underlying records |
| FR-014 | Provide watchlist and source-linked research notes | Alerts are not evaluated in Version 1 |
| FR-015 | Back up and restore database plus documents consistently | Manifest and checksum validation pass |
| FR-016 | Record and value manual simulated buys and sells without changing confirmed financial records | A synthetic multi-buy/partial-sell scenario reports source-labelled USD/THB P/L while confirmed cash, holdings, lots, and reconciliation remain unchanged |

Requirements for automatic corporate actions, advanced fundamentals, TWR/MWR, alerts, authentication, and AI remain roadmap items.
