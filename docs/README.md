# LedgerLens Documentation

Last reviewed: 2026-09-24.

LedgerLens has implemented local application source, automated tests, and planning documentation. The distributed foundation, initial confirmed-ledger slice, manual Simulation Accounts, and current API/handler foundations are implemented. IdentityAccess, interactive authentication, multi-user authorization, and non-loopback exposure remain proposed.

## Source-of-truth hierarchy

1. Source code and automated tests for implemented behaviour.
2. Accepted ADRs.
3. [Project State](project/PROJECT_STATE.md) for current implementation and verification status.
4. Architecture documents, whose explicit status distinguishes current and proposed design.
5. Product scope and requirements.
6. Active plans; each plan's own status controls whether work is proposed, in progress, implemented pending closeout, or complete.
7. Research evidence and unresolved recommendations.
8. Issue/change-request intake.
9. Historical or superseded material.

## Documentation map

- Product: [Executive Summary](product/EXECUTIVE_SUMMARY.md), [Vision](product/VISION.md), [Scope](product/SCOPE.md), [MVP](product/MVP.md), [Personas and Use Cases](product/PERSONAS_AND_USE_CASES.md), [Functional Requirements](product/FUNCTIONAL_REQUIREMENTS.md), [Non-functional Requirements](product/NON_FUNCTIONAL_REQUIREMENTS.md), [Roadmap](product/ROADMAP.md)
- Architecture: [System Overview](architecture/SYSTEM_OVERVIEW.md), [Aspire Topology](architecture/ASPIRE_TOPOLOGY.md), [Module Boundaries](architecture/MODULE_BOUNDARIES.md), [Domain Model](architecture/DOMAIN_MODEL.md), [Data Model](architecture/DATA_MODEL.md), [Investment Calculations](architecture/INVESTMENT_CALCULATIONS.md), [Currency and FX](architecture/CURRENCY_AND_FX.md), [Market Data](architecture/MARKET_DATA.md), [Slip Import](architecture/SLIP_IMPORT.md), [Research Center](architecture/RESEARCH_CENTER.md), [Security and Privacy](architecture/SECURITY_AND_PRIVACY.md), [Identity and Access](architecture/IDENTITY_ACCESS_AND_UI_LOCALIZATION.md), [Access Administration](architecture/ACCESS_ADMINISTRATION_AND_RESOURCE_CATALOG.md), [Session and MFA Administration](architecture/SESSION_AND_MFA_ADMINISTRATION.md), [Backup and Restore](architecture/BACKUP_AND_RESTORE.md), [Observability](architecture/OBSERVABILITY.md)
- Research: [Research Gate](research/PRE_IMPLEMENTATION_RESEARCH_GATE.md), [Platform](research/PLATFORM_AND_DEPENDENCY_EVALUATION.md), [Market Data](research/MARKET_DATA_PROVIDER_EVALUATION.md), [Calculations and FX](research/FINANCIAL_CALCULATION_AND_FX_EVALUATION.md), [Dime Extraction](research/DIME_SLIP_EXTRACTION_EVALUATION.md), [Localhost Security](research/LOCALHOST_SECURITY_EVALUATION.md), [Sources](research/sources/README.md), [Findings](research/findings/README.md), [Project Fit](research/project-fit/README.md)
- Decisions: [ADR index](adr/README.md)
- Quality: [Testing](quality/TESTING_STRATEGY.md), [Performance Budget](quality/PERFORMANCE_BUDGET.md), [Benchmark Plan](quality/BENCHMARK_PLAN.md), [Data Quality](quality/DATA_QUALITY_AND_RECONCILIATION.md)
- Project: [State](project/PROJECT_STATE.md), [Local Development Tooling](project/LOCAL_DEVELOPMENT_TOOLING.md), [Risks](project/RISK_REGISTER.md), [Release Strategy](project/RELEASE_STRATEGY.md), [Repository Safety](project/REPOSITORY_SAFETY_PLAN.md), [Dependencies](project/DEPENDENCY_AND_LICENSE_INVENTORY.md), [Open Questions](project/OPEN_QUESTIONS.md), [Traceability](project/REQUIREMENTS_TRACEABILITY.md), [Change Requests](project/ISSUE_AND_CHANGE_REQUESTS.md)
- Plans: [Plan index](plans/README.md), which lists every non-archived plan and its current status.

## Status vocabulary

- **Accepted**: evidence and owner direction are sufficient to implement.
- **Proposed**: preferred direction; validation remains.
- **Needs Evidence**: representative data or benchmark is missing.
- **Deferred**: intentionally outside the current release.
- **Superseded**: replaced by a later decision.
- **Implemented**: present in source, with any remaining verification/closeout gate stated explicitly.

## Documentation maintenance

- Keep volatile completion evidence in `project/PROJECT_STATE.md` and the owning plan instead of copying detailed status into multiple overview documents.
- Update `plans/README.md` whenever a plan is created, moved, or changes lifecycle state.
- When implementation changes an architecture statement from future to current, update that statement in the same change rather than only appending a newer paragraph.
- Run `lg docs` before committing documentation or milestone-state changes. It checks active-plan indexing, stale central-state phrases, review dates, and internal Markdown links.
