# LedgerLens Documentation

Last reviewed: 2026-08-11.

LedgerLens has no production implementation. All architecture and interfaces are proposed unless a document explicitly says otherwise.

## Source-of-truth hierarchy

1. Source code and automated tests, once they exist.
2. Accepted ADRs.
3. Architecture documents.
4. Product scope and requirements.
5. Active plans.
6. Research evidence and unresolved recommendations.
7. Issue/change-request intake.
8. Historical or superseded material.

## Documentation map

- Product: [Executive Summary](product/EXECUTIVE_SUMMARY.md), [Vision](product/VISION.md), [Scope](product/SCOPE.md), [MVP](product/MVP.md), [Personas and Use Cases](product/PERSONAS_AND_USE_CASES.md), [Functional Requirements](product/FUNCTIONAL_REQUIREMENTS.md), [Non-functional Requirements](product/NON_FUNCTIONAL_REQUIREMENTS.md), [Roadmap](product/ROADMAP.md)
- Architecture: [System Overview](architecture/SYSTEM_OVERVIEW.md), [Aspire Topology](architecture/ASPIRE_TOPOLOGY.md), [Module Boundaries](architecture/MODULE_BOUNDARIES.md), [Domain Model](architecture/DOMAIN_MODEL.md), [Data Model](architecture/DATA_MODEL.md), [Investment Calculations](architecture/INVESTMENT_CALCULATIONS.md), [Currency and FX](architecture/CURRENCY_AND_FX.md), [Market Data](architecture/MARKET_DATA.md), [Slip Import](architecture/SLIP_IMPORT.md), [Research Center](architecture/RESEARCH_CENTER.md), [Security and Privacy](architecture/SECURITY_AND_PRIVACY.md), [Backup and Restore](architecture/BACKUP_AND_RESTORE.md), [Observability](architecture/OBSERVABILITY.md)
- Research: [Research Gate](research/PRE_IMPLEMENTATION_RESEARCH_GATE.md), [Platform](research/PLATFORM_AND_DEPENDENCY_EVALUATION.md), [Market Data](research/MARKET_DATA_PROVIDER_EVALUATION.md), [Calculations and FX](research/FINANCIAL_CALCULATION_AND_FX_EVALUATION.md), [Dime Extraction](research/DIME_SLIP_EXTRACTION_EVALUATION.md), [Localhost Security](research/LOCALHOST_SECURITY_EVALUATION.md), [Sources](research/sources/README.md), [Findings](research/findings/README.md), [Project Fit](research/project-fit/README.md)
- Decisions: [ADR index](adr/README.md)
- Quality: [Testing](quality/TESTING_STRATEGY.md), [Performance Budget](quality/PERFORMANCE_BUDGET.md), [Benchmark Plan](quality/BENCHMARK_PLAN.md), [Data Quality](quality/DATA_QUALITY_AND_RECONCILIATION.md)
- Project: [State](project/PROJECT_STATE.md), [Local Development Tooling](project/LOCAL_DEVELOPMENT_TOOLING.md), [Risks](project/RISK_REGISTER.md), [Release Strategy](project/RELEASE_STRATEGY.md), [Repository Safety](project/REPOSITORY_SAFETY_PLAN.md), [Dependencies](project/DEPENDENCY_AND_LICENSE_INVENTORY.md), [Open Questions](project/OPEN_QUESTIONS.md), [Traceability](project/REQUIREMENTS_TRACEABILITY.md), [Change Requests](project/ISSUE_AND_CHANGE_REQUESTS.md)
- Plans: [Plan index](plans/README.md), [Portfolio Core Vertical Slices](plans/active/0001-portfolio-core-vertical-slices.md)

## Status vocabulary

- **Accepted**: evidence and owner direction are sufficient to implement.
- **Proposed**: preferred direction; validation remains.
- **Needs Evidence**: representative data or benchmark is missing.
- **Deferred**: intentionally outside the current release.
- **Superseded**: replaced by a later decision.
