# Aspire Topology

Status: Accepted foundation direction. Baseline researched on 2026-08-11.

## Resource graph

| Resource | Responsibility | Dependencies | Exposure |
| --- | --- | --- | --- |
| AppHost | Resource graph, parameters, references, startup and Dashboard | All resources | Developer process only |
| Angular Web | Connectivity shell now; product UI later | Gateway | Loopback |
| Gateway | YARP routes, future BFF/OIDC boundary | All APIs | Loopback; only browser-facing backend |
| Portfolio Core API | Future portfolio/ledger/lots atomic boundary | portfolio database, NATS | Internal |
| Market Data API | Future quotes/FX/provider boundary | market database, NATS | Internal |
| Slip Import API | Future document/review boundary | slip database, NATS | Internal |
| Slip Import Worker | Future OCR/background execution | slip database, NATS | No HTTP exposure |
| Research API | Future research boundary | research database, NATS | Internal |
| Operations API | Future backup/restore coordinator | operations database, NATS | Internal |
| PostgreSQL 18 | One local server with five service-owned databases | external runtime volume | Internal |
| NATS JetStream | At-least-once integration messaging | external runtime volume | Internal |
| Aspire Dashboard | Structured logs, traces, metrics and health | OTLP from resources | Loopback, development/local use |

AppHost grants discovery/configuration only through explicit references. Gateway references APIs. Each API references only its database and NATS. Slip Worker references only Slip database and NATS. Databases are separate resources even when hosted by one PostgreSQL container.

## Shared technical defaults

Every .NET executable references `LedgerLens.ServiceDefaults`, which configures OpenTelemetry, health checks, Aspire service discovery, and standard `HttpClient` resilience. It contains no domain models, event contracts, persistence helpers, or business utilities. `LedgerLens.IntegrationContracts` contains only the transport envelope and future versioned integration contracts.

## Startup policy

The foundation starts the complete graph so connectivity and resource cost are known before business logic. Only Angular and Gateway receive external loopback endpoints. Runtime volumes must resolve outside the repository. Dynamic service ports are expected; callers use logical service names.

## Evidence

- [Communication evaluation](../research/ASPIRE_INTERSERVICE_COMMUNICATION_EVALUATION.md)
- [Local Development Tooling](../project/LOCAL_DEVELOPMENT_TOOLING.md)

