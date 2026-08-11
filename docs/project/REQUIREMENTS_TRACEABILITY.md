# Requirements Traceability

Last reviewed: 2026-08-11.

The source brief is `../../../LedgerLens_Codex_Planning_Context.md` relative to this document and remains unchanged.

| Brief area | Primary documentation |
| --- | --- |
| Executive summary | [Executive Summary](../product/EXECUTIVE_SUMMARY.md) |
| Vision, local deployment, no trading advice | [Vision](../product/VISION.md), [Scope](../product/SCOPE.md) |
| Stack and distributed architecture | [Platform Research](../research/PLATFORM_AND_DEPENDENCY_EVALUATION.md), [Communication Research](../research/ASPIRE_INTERSERVICE_COMMUNICATION_EVALUATION.md), [System](../architecture/SYSTEM_OVERVIEW.md), ADR-0001, ADR-0003, ADR-0015 |
| Clean Code, handlers, Code First | [.NET Architecture](../architecture/DOTNET_APPLICATION_ARCHITECTURE.md), ADR-0014 |
| No authentication / fixed local user / future OIDC | [.NET Architecture](../architecture/DOTNET_APPLICATION_ARCHITECTURE.md), [Security](../architecture/SECURITY_AND_PRIVACY.md), ADR-0012 |
| Portfolio/account/transaction capabilities | [Functional Requirements](../product/FUNCTIONAL_REQUIREMENTS.md), [Domain Model](../architecture/DOMAIN_MODEL.md) |
| Calculations, precision, FIFO/average, TWR/MWR | [Calculation Specification](../architecture/INVESTMENT_CALCULATIONS.md), ADR-0004–0006 |
| Dime workflow | [Slip Import](../architecture/SLIP_IMPORT.md), [Dime Research](../research/DIME_SLIP_EXTRACTION_EVALUATION.md), ADR-0009 |
| Dashboard and holdings | [MVP](../product/MVP.md), [Functional Requirements](../product/FUNCTIONAL_REQUIREMENTS.md) |
| Quotes, freshness, providers and FX | [Market Data](../architecture/MARKET_DATA.md), [Provider Research](../research/MARKET_DATA_PROVIDER_EVALUATION.md), ADR-0007–0008 |
| Research | [Research Center](../architecture/RESEARCH_CENTER.md) |
| Data ownership and ERD | [Service Boundaries](../architecture/MODULE_BOUNDARIES.md), [Data Model](../architecture/DATA_MODEL.md) |
| Privacy, secrets and runtime data | [Repository Safety](REPOSITORY_SAFETY_PLAN.md), [Security](../architecture/SECURITY_AND_PRIVACY.md), ADR-0010–0011 |
| Correctness, performance and observability | [Testing](../quality/TESTING_STRATEGY.md), [Budget](../quality/PERFORMANCE_BUDGET.md), [Observability](../architecture/OBSERVABILITY.md) |
| Roadmap and milestones | [Roadmap](../product/ROADMAP.md), [Active Plan](../plans/active/0001-portfolio-core-vertical-slices.md) |
| Risks, releases and dependencies | [State](PROJECT_STATE.md), [Risks](RISK_REGISTER.md), [Release](RELEASE_STRATEGY.md), [Dependencies](DEPENDENCY_AND_LICENSE_INVENTORY.md) |

