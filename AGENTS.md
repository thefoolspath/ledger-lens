# AGENTS.md

## Project state

LedgerLens currently contains planning documentation only. Proposed interfaces and architecture are not implemented behaviour.

## Repository map

- `docs/product/`: vision, scope, requirements, MVP, and roadmap.
- `docs/architecture/`: proposed system, data, security, and operational design.
- `docs/research/`: evidence, findings, and decision gates.
- `docs/adr/`: architecture decision records.
- `docs/plans/active/`: executable future implementation plans.
- `docs/quality/`: correctness, testing, performance, and reconciliation.
- `docs/project/`: state, risks, release strategy, safety, dependencies, and traceability.

## Required workflow

1. Read `docs/project/PROJECT_STATE.md` and the relevant active plan.
2. Read applicable product, architecture, research, ADR, and quality documents.
3. Confirm time-sensitive claims against official sources before implementation.
4. Keep real user data and secrets outside the repository.
5. Use only newly created synthetic data in tracked files and test artifacts.
6. Implement the smallest vertical milestone and verify its acceptance criteria.
7. For every repository change, update at least one relevant Markdown (`.md`) document in the same task, whether the change was made by the user and reported to the agent or made by the agent. When the user reports their own change, inspect that change and update the relevant documentation before considering the task complete.
8. Update `docs/project/PROJECT_STATE.md` when verified behaviour or project state changes; not every repository change requires a `PROJECT_STATE.md` update.
9. A documentation-only change satisfies the Markdown-update requirement through the document being changed and does not require a second, recursive documentation entry.

## Decision rules

- Research precedes a costly or difficult-to-reverse dependency decision.
- An ADR is `Accepted` only when its evidence gate is complete.
- Code and automated tests become the source of truth for implemented behaviour.
- Financial calculations must use exact decimal arithmetic and deterministic rules; an LLM is never authoritative for numbers.
- Public API keys stay in the backend/worker and must never reach Angular.
- Do not expose any service, database, dashboard, or development endpoint beyond loopback before the authentication security gate passes.

## Definition of done

Acceptance criteria, relevant tests, privacy checks, documentation updates, and reconciliation invariants pass without introducing unrelated changes or real personal data. Work is not complete until every repository change is reflected in at least one relevant Markdown document under the workflow rules above.
