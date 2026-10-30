# 0007.07 — Decompose oversized persistence ports without adding a generic repository

Last reviewed: 2026-09-28.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: Maintainability, persistence ownership, and transactions

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [0007.03](03-semantic-naming-consistency.md) — capability and port names must be approved from actual responsibilities.
- [0007.04](04-collection-correctness-pagination-and-sorting.md) — page/query contracts and complete-ledger correctness must stabilize first.
- [0007.06](06-request-lifecycle-timeouts-mutations-and-exports.md) — transaction ownership, cancellation token handoff, retry, and idempotency must remain intact.


**Status:** Pending owner decision after correctness and contract migrations.

### Current evidence and concern

`PortfolioCoreStore` currently implements both `IPortfolioCoreStore` and `ISimulationStore` and owns Portfolio, Investment Account, Cash Ledger, Simulation Account, draft, trade, valuation, mapping, query, and transaction behavior. `ISimulationStore` alone spans accounts, drafts, trades, calculation inputs, and valuation snapshots. Pagination and export work would make these broad interfaces and the single Infrastructure class grow further, increasing change coupling and weakening interface segregation.

### Proposed direction

- Split persistence implementations and ports by cohesive business capability, such as Portfolio, Investment Accounts, Cash Ledger, Simulation Accounts/Trades, and Simulation Valuations.
- Keep operation-specific query shape, tracking, ownership, ordering, and transaction behavior visible in intention-revealing methods.
- Keep one scoped `PortfolioCoreDbContext`; do not wrap every EF operation or create a generic CRUD repository.
- Keep each atomic transaction inside one cohesive persistence boundary. Do not split a transaction across several stores merely to achieve smaller files.
- Introduce the split in bounded feature stages after characterization/contract tests exist; do not combine it with calculation changes, schema changes, or a repository-wide rename.

### Acceptance criteria if accepted

- Handler dependencies expose only the operations they require; no replacement god interface appears.
- Transaction, retry execution strategy, tracking, ownership, query ordering, cancellation, and externally observable results remain unchanged.
- No service locator, runtime dispatcher, reflection registration, generic repository, or cross-context transaction is introduced.
- Store-focused and operation integration tests pass together with architecture, EF model-drift, privacy, and `git diff --check` gates.

### Decision record

- **Decision:** Pending.
- **Owner rationale:** Pending review after the correctness, typed-failure, and page-contract shapes stabilize.
- **Decision date:** Pending.
- **Implementation evidence:** Not started.
