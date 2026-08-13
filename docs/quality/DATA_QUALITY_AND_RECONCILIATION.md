# Data Quality and Reconciliation

Status: Proposed.

## Invariants

- Confirmed ledger replay is deterministic.
- Account cash equals signed cash movements after fees, taxes, dividends, and FX conversions.
- Sum of remaining USD cash-lot quantities equals ledger-derived USD cash by account; linked internal transfers preserve quantity, carrying rate, and provenance across accounts.
- Every USD consumption is fully allocated by evidence, manual confirmation, or deterministic FIFO, or the affected result is explicitly incomplete.
- Holding quantity equals acquisitions plus adjustments minus disposals.
- Sum of lot remaining quantities equals holding quantity.
- Sum of sale allocations equals sold quantity and cannot exceed lot remainder.
- FIFO and average views use the same remaining allocated cost total.
- Stock effect plus FX effect equals total THB P/L before display rounding.
- Primary and BOT benchmark columns each reconcile independently and are never added together.
- Exchange execution plus pre-trade cash FX plus stock effect plus stock FX plus post-sale cash FX plus exchange-back execution equals actual closed-cycle THB proceeds minus contributions before display rounding.
- A posted slip maps to one idempotent posting; corrections remain linked.
- Every external value has source, as-of, retrieval time, and freshness.

## Reconciliation outputs

Provide per-account cash/quantity exceptions, orphan slips/documents, missing FX/prices, unknown external-USD basis, incomplete or conflicting cash-lot allocations, duplicate provider points, stale projections, and backup manifest mismatches. Never silently repair financial history; propose a reviewable correction.

## Snapshot rule

Snapshots accelerate reads but are not authoritative. Periodically rebuild from the ledger and compare hash/totals; a mismatch blocks release and marks affected views unavailable until diagnosed.
