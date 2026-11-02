# Code Review Decision Log

Last reviewed: 2026-10-30. Status: Active decision index; each child record carries its own implementation and verification status.

## Goal

Classify restructuring and correctness concerns discovered during code review without turning one large document into an implementation backlog. Each child record owns one cohesive problem area, states its relationships and prerequisites, and preserves the evidence, options, decision, and acceptance criteria needed before source changes begin.

This index is authoritative for cross-item relationships and delivery order. The child records are decision documents, not implementation authorization. An accepted item must be delivered through its named existing plan or a separately approved implementation plan.

## Review workflow

1. Review one decision record at a time against the current source.
2. Record the owner decision as `Accepted`, `Rejected`, or `Deferred`; the default is `Pending`.
3. Do not treat discussion, an initial preference, or inclusion in this index as implementation approval.
4. Resolve every listed prerequisite and cross-item contract before source changes begin.
5. For an accepted item, update the affected source, tests, and governing documentation together under its implementation-owning plan.
6. Verify the smallest relevant build and tests, then record exact evidence in both the decision record and implementation plan.
7. Correctness and data-integrity defects take precedence over structural cleanup.
8. An accepted direction with unresolved contract, migration, timeout, or performance values remains blocked at that unresolved gate; acceptance is not permission to invent the missing value during implementation.

## Decision groups

### API contract and failure boundary

| ID | Decision record | Status | Implementation owner | Primary relationships |
| --- | --- | --- | --- | --- |
| `0007.01` | [Created and Location policy](0007/01-created-location-policy.md) | Implemented; distributed create-and-follow passed | Focused 0007.01 implementation | `0007.02`, `0007.04`, `0007.06` |
| `0007.02` | [Typed failures and exception handling](0007/02-typed-failures-and-exception-handling.md) | Accepted direction; not implemented | [Plan 0004](0004-unified-api-response-contract.md) | `0007.01`, `0007.04`, `0007.06` |

### Financial correctness and collection contracts

| ID | Decision record | Status | Implementation owner | Primary relationships |
| --- | --- | --- | --- | --- |
| `0007.04` | [Collection correctness, pagination, and sorting](0007/04-collection-correctness-pagination-and-sorting.md) | Complete-ledger cap removed in source; distributed regression pending; pagination not started | Focused 0007.04 implementation | `0007.02`, `0007.03`, `0007.05`, `0007.06`, `0007.07` |

The complete-ledger `.Take(5000)` defect is the first correctness gate inside `0007.04`. It must be corrected and covered by an over-5,000-entry regression scenario before pagination or structural refactoring proceeds.

### Frontend structure and visual system

| ID | Decision record | Status | Implementation owner | Primary relationships |
| --- | --- | --- | --- | --- |
| `0007.05` | [Angular feature architecture and SCSS system](0007/05-angular-feature-architecture-and-scss-system.md) | Accepted as separately verified Milestones 5A and 5B; not implemented | Future focused implementation plans | `0007.03`, `0007.04`, `0007.06` |

Milestone 5A owns behavior-preserving Angular feature extraction. Milestone 5B owns the SCSS/design-system migration and starts only after extracted features demonstrate reusable visual contracts. Pagination is implemented inside the extracted Simulation feature after the Milestone 5A boundary exists.

### Operational lifecycle and failure modes

| ID | Decision record | Status | Implementation owner | Primary relationships |
| --- | --- | --- | --- | --- |
| `0007.06` | [Request lifecycle, timeouts, mutations, and exports](0007/06-request-lifecycle-timeouts-mutations-and-exports.md) | Accepted design direction; thresholds remain evidence-gated | Future focused implementation plans | `0007.01`, `0007.02`, `0007.04`, `0007.05`, `0007.07` |

Read cancellation and measured timeout policies may be delivered with affected vertical slices. Mutation token handoff requires an operation-specific atomicity and idempotency decision first. Export implementation remains deferred until the first export use case and measured synchronous/asynchronous threshold are approved.

### Maintainability and dependency clarity

| ID | Decision record | Status | Implementation owner | Primary relationships |
| --- | --- | --- | --- | --- |
| `0007.03` | [Semantic naming consistency](0007/03-semantic-naming-consistency.md) | Pending assessment and owner decision | Assessment first; bounded plans only after approval | `0007.04`, `0007.05`, `0007.07` |
| `0007.07` | [Persistence port decomposition](0007/07-persistence-port-decomposition.md) | Pending after correctness and contract migrations | Future focused implementation plan | `0007.03`, `0007.04`, `0007.06` |

Naming assessment is not a correctness blocker. Correct the governing C# casing documentation independently, then approve only cohesive rename batches. Persistence-port decomposition waits until page/query contracts, transaction ownership, cancellation, and typed failures stabilize.

## Relationship map

| From | Depends on or constrains | Relationship |
| --- | --- | --- |
| `0007.01` Location | `0007.02`, `0007.06` | Success/failure responses and asynchronous status URLs must expose truthful public Gateway routes. |
| `0007.02` Typed failures | Plan 0004, `0007.04`, `0007.06` | Page validation, dependency failures, cancellation, timeout, and unexpected failures share one transport boundary but retain distinct semantics. |
| `0007.03` Naming | `0007.04`, `0007.05`, `0007.07` | Cardinality, feature ownership, and persistence capability names must follow the final contracts rather than lead them. |
| `0007.04` Collections | `0007.05`, `0007.06`, `0007.07` | Backend page contracts require matching Angular state, cancellation/resource bounds, and stable query ownership. |
| `0007.05` Angular | `0007.04`, `0007.06` | Extracted features own page/query state and latest-request cancellation; SCSS work follows structural extraction. |
| `0007.06` Lifecycle | `0007.02`, `0007.07` | Failure classification and mutation transaction/token ownership must be fixed before changing store boundaries. |
| `0007.07` Persistence ports | `0007.03`, `0007.04`, `0007.06` | Split only after capability names, query shapes, and transaction semantics stabilize. |

## Cross-item dependency and delivery order

1. Correct the complete-ledger `.Take(5000)` defect under `0007.04` and add the over-5,000-entry regression evidence before structural refactoring.
2. Correct the governing C# casing documentation under `0007.03`; continue the wider naming inventory in parallel without blocking correctness work.
3. The distributed create-and-follow gate for the implemented per-endpoint `Location` decisions under `0007.01` passed on 2026-10-25.
4. Deliver typed expected failures under plan 0004 and `0007.02` before removing corresponding endpoint catches.
5. Capture Angular characterization tests and deliver the behavior-preserving Milestone 5A feature extraction under `0007.05`.
6. Implement `SimulationAccountsGetPage` under `0007.04` inside the extracted Simulation feature, including Angular route/query state.
7. Migrate Portfolio, Investment Account, Cash Ledger, and nested collection contracts in bounded vertical stages.
8. Apply `0007.06` read cancellation and measured timeout policies with affected vertical stages; define mutation token handoff per operation before changing financial mutation behavior.
9. Review and, if accepted, implement `0007.07` persistence-port decomposition alongside already-tested feature stages.
10. Build Milestone 5B tokens and primitives from demonstrated extracted-feature usage, then remove Tailwind only after the final visual and build gates.
11. Implement export-specific code under `0007.06` only after the first export milestone, resource budget, storage lifecycle, and synchronous/asynchronous decision are accepted.

Items that do not touch the same contracts may proceed independently. No item may claim completion while its prerequisite behavior, decision record, implementation plan, or acceptance evidence remains pending.

## Promotion to implementation

Before a child decision record produces source changes, record:

- the existing or new implementation-owning plan;
- accepted scope and explicit exclusions;
- preserved public contracts and data invariants;
- migration and rollback or containment steps;
- focused automated and runtime acceptance evidence;
- documentation and `PROJECT_STATE.md` updates required after verified behavior changes.

When all child records are either implemented through their owning plans, rejected, or explicitly deferred with revisit triggers, archive this index together with the remaining decision records.
