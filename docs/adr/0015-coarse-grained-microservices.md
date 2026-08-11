# ADR-0015: Coarse-Grained Microservices with HTTP and NATS

## Status

Accepted for Version 1 foundation.

## Context

LedgerLens needs the complete distributed topology to run and communicate before business logic is added. Aspire supplies orchestration, discovery, dependency configuration, health, and OpenTelemetry, while LedgerLens financial transactions still require strong local consistency.

## Decision

Create independent Gateway, Portfolio Core, Market Data, Slip Import API/worker, Research, and Operations deployables. Portfolio, ledger, lots, cash, and financial calculation remain together in Portfolio Core. Use ASP.NET Core/YARP Gateway as the only browser-facing backend. Use Aspire service discovery and HTTP/JSON for synchronous request/response, and NATS JetStream for durable asynchronous integration after business slices introduce events.

Run one PostgreSQL server locally but allocate a separate database and EF Core `DbContext` to each service. A service must not access another service's database, schema, entities, or migrations. Cross-service data flows only through versioned HTTP contracts or integration events. Do not use distributed transactions.

Use Aspire Dashboard for Version 1 technical logs, traces, metrics, and health. Keep business audit records in their owning service rather than treating technical logs as financial evidence.

## Alternatives considered

Modular monolith, service per CRUD module, HTTP-only communication, gRPC plus NATS, shared database, and separate logging/monitoring products.

## Evidence

[Aspire communication evaluation](../research/ASPIRE_INTERSERVICE_COMMUNICATION_EVALUATION.md), [Aspire topology](../architecture/ASPIRE_TOPOLOGY.md), and [data model](../architecture/DATA_MODEL.md).

## Consequences

Failure and dependency boundaries become explicit and independently deployable. The foundation has greater project, container, testing, migration, backup, and eventual-consistency cost. Keeping Portfolio Core coarse prevents the highest-risk distributed financial transactions.

## Risks

Excessive synchronous chains, shared contract leakage, duplicated messages, cross-database reads, and a large local resource footprint.

## Revisit conditions

Merge or split a service only when measured coupling, lifecycle, isolation, scale, or reliability evidence justifies it. gRPC requires a streaming or throughput benchmark.

