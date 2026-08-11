# ADR-0005: Dual Cost-basis Views

## Status

Accepted by owner direction.

## Context

The owner wants both FIFO and average-cost views. A second ledger would create conflicting truth.

## Decision

Maintain one confirmed ledger and explicit trade lots/allocations. Provide FIFO and average cost as equally visible, testable first-class reports. A sale proposes FIFO allocation by default but stores the confirmed allocation. Average cost is analytical and not tax advice.

## Alternatives considered

FIFO only, average only, and two independently stored cost ledgers.

## Evidence

[Calculation evaluation](../research/FINANCIAL_CALCULATION_AND_FX_EVALUATION.md) and [calculation specification](../architecture/INVESTMENT_CALCULATIONS.md).

## Consequences

Users can compare views while reconciliation remains anchored to lots; more calculations and labels require testing.

## Risks

Users may mistake average cost for a tax method.

## Revisit conditions

Revisit labels and jurisdiction-specific exports after legal/tax requirements are explicitly scoped.
