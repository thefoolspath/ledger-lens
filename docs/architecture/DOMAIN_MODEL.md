# Domain Model

Status: Proposed.

```text
Future Principal
└── Portfolio (ownership boundary)
    ├── Investment Account
    │   ├── Ledger Entries
    │   ├── Trade Lots and Allocations
    │   └── Slip Documents and Reviews
    ├── Watchlists and Research Notes
    └── Reporting Preferences (USD base, THB reporting)
```

## Aggregates

- **Portfolio**: name, base/reporting currencies, accounts, and reporting policy.
- **InvestmentAccount**: broker identity and account-scoped ledger.
- **LedgerEntry**: immutable economic event plus correction links and source provenance.
- **TradeLot**: acquisition quantity and allocated cost; allocations connect disposals to lots.
- **SlipDocument**: immutable original identity, storage metadata, extraction attempts, reviews, and posted-entry links.
- **ResearchNote/Thesis**: versioned opinion separated from external facts and sources.

## Invariants

- Confirmed quantity cannot become negative unless an explicitly supported short-position feature is added.
- A correction references what it corrects; it does not erase the prior entry.
- Sum of remaining lots equals the ledger-derived holding quantity.
- Posting a reviewed slip is idempotent.
- A fixed local current user owns one or more Portfolios through required `Portfolio.OwnerUserId`; descendants inherit this ownership boundary.
- The internal `UserProfile.Id` is relational identity. A provider identity or email is never a domain primary key, and future OIDC mapping uses issuer plus subject rather than email.

Corporate actions are modelled as explicit ledger adjustments with source and effective date; automated ingestion is deferred.
