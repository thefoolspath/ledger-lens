# Domain Model

Status: Partially implemented; manual simulation aggregates and calculations are implemented, while later confirmed-investment aggregates remain proposed.

```text
Future Principal
└── Portfolio (ownership boundary)
    ├── Investment Account
    │   ├── Ledger Entries
    │   ├── Trade Lots and Allocations
    │   └── Slip Documents and Reviews
    ├── Watchlists and Research Notes
    ├── Simulation Accounts
    │   ├── Simulation Trade Entries
    │   ├── Simulated Lots and Allocations
    │   └── Valuation Snapshots
    └── Reporting Preferences (USD base, THB reporting)
```

## Aggregates

- **Portfolio**: name, base/reporting currencies, accounts, and reporting policy.
- **InvestmentAccount**: broker identity and account-scoped ledger.
- **LedgerEntry**: immutable economic event plus correction links and source provenance.
- **TradeLot**: acquisition quantity and allocated cost; allocations connect disposals to lots.
- **SlipDocument**: immutable original identity, storage metadata, extraction attempts, reviews, and posted-entry links.
- **ResearchNote/Thesis**: versioned opinion separated from external facts and sources.
- **SimulationAccount**: manually entered hypothetical activity with no funded cash ledger and no relationship to confirmed Investment Account balances.
- **SimulationTradeEntry**: immutable buy/sell or linked correction with exact assumptions and market/FX provenance.

## Invariants

- Confirmed quantity cannot become negative unless an explicitly supported short-position feature is added.
- A correction references what it corrects; it does not erase the prior entry.
- Sum of remaining lots equals the ledger-derived holding quantity.
- Posting a reviewed slip is idempotent.
- A fixed local current user owns one or more Portfolios through required `Portfolio.OwnerUserId`; descendants inherit this ownership boundary.
- Simulation quantity, lots, and valuation rebuild only from simulation entries; no simulation row contributes to a confirmed cash, holding, lot, or reconciliation query.
- The internal `UserProfile.Id` is relational identity. A provider identity or email is never a domain primary key, and future OIDC mapping uses issuer plus subject rather than email.

Corporate actions are modelled as explicit ledger adjustments with source and effective date; automated ingestion is deferred.
