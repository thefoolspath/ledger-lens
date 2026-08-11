# ADR-0004: Auditable Ledger Corrections

## Status

Accepted.

## Context

Editing a historical transaction in place can invalidate lots, P/L, slip traceability, and audit explanations.

## Decision

Confirmed economic events are append-only. A correction records a reversal/replacement relationship, actor/reason/time/source, and triggers deterministic projection rebuild. Draft and extraction records may be edited before posting.

## Alternatives considered

In-place update, full event sourcing for every UI state, and periodic snapshot as authority.

## Evidence

[Domain model](../architecture/DOMAIN_MODEL.md) and [data quality strategy](../quality/DATA_QUALITY_AND_RECONCILIATION.md).

## Consequences

History and reconciliation remain explainable; correction UX and projections are more complex.

## Risks

Duplicate reversal, correction chains, or stale projections.

## Revisit conditions

Revisit projection mechanics after performance measurements, not the requirement to preserve economic history.
