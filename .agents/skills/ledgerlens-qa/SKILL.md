---
name: ledgerlens-qa
description: Run evidence-based quality assurance for the LedgerLens repository. Use when asked to test LedgerLens, verify that the project or a milestone works, perform a smoke or regression pass, inspect release readiness, validate acceptance criteria, test the Angular UI through a browser or Windows automation, or produce a QA defect report.
---

# LedgerLens QA

Test only behavior that the repository says is implemented. Treat plans and proposed interfaces as requirements, not executable behavior.

## Establish the test contract

1. Read `AGENTS.md`, `docs/project/PROJECT_STATE.md`, and the relevant file under `docs/plans/active/` completely.
2. Read the acceptance criteria and applicable product, architecture, ADR, research, and quality documents. Always include `docs/quality/TESTING_STRATEGY.md`; add reconciliation, performance, security, or privacy documents when the requested scope touches them.
3. Inspect the working tree before testing. Preserve user changes and do not repair defects unless the user also asks for implementation.
4. Build a scope matrix with each requirement marked `In scope`, `Blocked by gate`, or `Not implemented`. Never fail a future milestone merely because its behavior does not exist yet.
5. Use only newly created synthetic inputs. Keep real financial data, credentials, local runtime state, screenshots containing private data, and secrets outside tracked files.

When documents disagree, report the conflict instead of silently choosing one. Use current code and automated tests as the source of truth for implemented behavior; use accepted requirements, ADRs, and the active plan as the test oracle. If Git metadata is unavailable, record the filesystem snapshot time and explicitly state that commit and working-tree identity could not be verified.

## Run the cheapest decisive gates first

Run commands from the repository root through the project wrappers:

```powershell
.\scripts\dev.ps1 doctor
.\scripts\dev.ps1 build
.\scripts\dev.ps1 test
```

- Do not restore or bootstrap unless dependencies are missing and the user authorized a change or download.
- Record command, exit code, meaningful counts, warnings, duration when available, and decisive output. Do not infer a pass from old `bin`, `obj`, `dist`, or test-result files.
- Classify a tool or environment failure as `BLOCKED`, not a product failure, when evidence shows the product code was not exercised.
- Stop broad follow-on testing after a build failure when later results would be misleading; isolate the smallest relevant test when useful for diagnosis.

## Verify foundation invariants

For Milestone 1, verify the active plan rather than generic application expectations:

- Solution and Angular production builds complete without warnings or errors required by the gate.
- Architecture, Gateway, service, and Angular tests pass.
- Angular calls the Gateway only; browser-facing endpoints remain loopback-only.
- No business type, table, or event has entered the foundation milestone.
- No `EnsureCreated`, cross-service internal reference, cross-database access, public key in Angular, mediator, mapper, validation framework, generic repository, or extra unit-of-work framework violates the accepted design.
- Tracked files do not contain secrets, real personal data, or runtime investment data.

Use targeted repository searches and source inspection for invariants not covered by an automated test. Report the exact search scope and distinguish absence of a match from proof of runtime behavior.

## Run distributed and UI QA conditionally

Run `.\scripts\dev.ps1 run` and distributed tests only when Docker client and server checks pass and the current milestone requires the runtime gate. Keep all listeners on loopback.

When the graph is healthy:

1. Exercise Gateway-to-service routes using dynamic discovered endpoints.
2. Confirm PostgreSQL and NATS JetStream readiness and persistence expectations.
3. Confirm one request produces correlated logs, traces, and metrics in the Aspire Dashboard.
4. Test the Angular connectivity shell in its healthy, degraded, unavailable, loading, and recovery states where controllable.
5. Check keyboard access, focus visibility, semantic status announcements, text readability, responsive layout, and browser console/network errors.

Use a browser automation skill for web semantics when available. Use Computer Use only for Windows UI or an existing signed-in/local app state, and read that skill's required guidance and confirmation rules before acting. Capture screenshots only when they add evidence and contain synthetic or non-sensitive content.

If Docker or another required dependency is unavailable, collect the error, mark affected cases `BLOCKED`, continue all independent static/build/unit/component gates, and state exactly what remains unverified.

## Apply financial and data rules

When a business milestone is active:

- Require exact decimal arithmetic, deterministic fixtures, and explicit rounding/currency rules.
- Verify ledger transitions, corrections, replay, lots, cost bases, FX attribution, and reconciliation invariants applicable to the milestone.
- Include negative, boundary, duplicate, retry, cancellation, and idempotency cases.
- Never use an LLM-produced number as the expected financial result; derive expectations from documented rules and deterministic code or hand-verifiable arithmetic.
- Do not claim exactly-once messaging. Test Outbox/Inbox crash windows, duplicates, ordering, poison-message handling, version compatibility, and safe replay when those features activate.

## Report results

Use only these case statuses:

- `PASS`: current execution directly satisfied the expected result.
- `FAIL`: current execution exercised the product and contradicted the expected result.
- `BLOCKED`: an external prerequisite prevented execution.
- `NOT APPLICABLE`: outside the active milestone or accepted scope.
- `NOT RUN`: in scope but intentionally not executed; always give the reason.

Lead with the release or milestone recommendation: `Go`, `Conditional go`, or `No-go`. Then provide:

1. Tested commit or working-tree state and environment.
2. Scope and acceptance-criteria coverage.
3. Commands and evidence.
4. Defects ordered by severity, each with title, expected, actual, reproducible steps, impact, and evidence location.
5. Blocked and untested risks.
6. A concise retest checklist.

Do not say "everything works" unless every in-scope acceptance criterion has current direct evidence. A passing automated suite does not close manual runtime, observability, accessibility, privacy, or reconciliation gates that the plan requires.

## Repository changes during QA

Default to read-only QA. If the user asks for test code, fixtures, or fixes, implement the smallest vertical change, verify it, and update at least one relevant Markdown document in the same task as required by `AGENTS.md`. Update `docs/project/PROJECT_STATE.md` only when verified behavior or project state changes.

