# Aspire Topology

Status: Accepted and partially implemented. Last reviewed: 2026-09-24.

## Resource graph

| Resource | Responsibility | Dependencies | Exposure |
| --- | --- | --- | --- |
| AppHost | Resource graph, parameters, references, startup and Dashboard | All resources | Developer process only |
| Angular Web | Implemented connectivity, confirmed-ledger workflow, and Simulation mode; later product UI continues here | Gateway | Loopback |
| Gateway | Implemented YARP routes and local request controls; proposed BFF/OIDC boundary | All current APIs; proposed IdentityAccess | Loopback; only browser-facing backend |
| IdentityAccess API (proposed) | external identity mapping, sessions, permissions, UI resource localization | identity database | Internal |
| Portfolio Core API | Implemented portfolio/account/cash-ledger and isolated Simulation slices; later confirmed ledger/lots remain | portfolio database, NATS | Internal |
| Market Data API | Implemented provider-neutral quote/FX/candle operations and development provider; later provider/event coverage remains | market database, NATS | Internal |
| Slip Import API | Implemented foundation host; document/review behaviour remains proposed | slip database, NATS | Internal |
| Slip Import Worker | Implemented foundation host; OCR/background behaviour remains proposed | slip database, NATS | No HTTP exposure |
| Research API | Implemented foundation host; research behaviour remains proposed | research database, NATS | Internal |
| Operations API | Implemented foundation host; backup/restore coordination remains proposed | operations database, NATS | Internal |
| PostgreSQL 18 | One local server with five current service-owned databases and one proposed identity database | external runtime volume | Internal |
| NATS JetStream | At-least-once integration messaging | external runtime volume | Internal |
| Aspire Dashboard | Structured logs, traces, metrics and health | OTLP from resources | Loopback, development/local use |

AppHost grants discovery/configuration only through explicit references. Gateway references APIs. Each API references only its database and NATS. Slip Worker references only Slip database and NATS. Databases are separate resources even when hosted by one PostgreSQL container.

If the proposed identity plan is accepted, Gateway also references IdentityAccess and external key/configuration parameters; IdentityAccess references only its database and required technical defaults. Angular keeps one stable interactive loopback origin for Google registration, while distributed tests retain dynamic isolated endpoints and synthetic authentication.

The proposed local graph also includes an `identity-catalog-sync` maintenance resource. Its custom Dashboard/CLI commands preview and apply the source-controlled UI resource manifest through IdentityAccess handlers. It has no external endpoint, is not part of production request traffic, and cannot make the Aspire Dashboard a production administration surface.

## Shared technical defaults

Every .NET executable references `LedgerLens.ServiceDefaults`, which configures OpenTelemetry, health checks, Aspire service discovery, and standard `HttpClient` resilience. It contains no domain models, event contracts, persistence helpers, or business utilities. `LedgerLens.IntegrationContracts` contains only the transport envelope and future versioned integration contracts.

## Startup policy

AppHost starts the complete current graph so connectivity and resource cost remain observable as business slices are added. Only Angular and Gateway receive external loopback endpoints. Runtime volumes must resolve outside the repository. Dynamic service ports are expected; callers use logical service names.

## Evidence

- [Communication evaluation](../research/ASPIRE_INTERSERVICE_COMMUNICATION_EVALUATION.md)
- [Local Development Tooling](../project/LOCAL_DEVELOPMENT_TOOLING.md)
