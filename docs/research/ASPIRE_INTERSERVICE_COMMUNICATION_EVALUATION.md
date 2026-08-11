# Aspire Interservice Communication Evaluation

Research date: 2026-08-11. Decision scope: LedgerLens distributed foundation.

## Official findings

- Aspire `WithReference` supplies explicit service-discovery configuration. .NET clients use logical names such as `https+http://portfolio-core` instead of fixed ports.
- The C# ServiceDefaults template configures OpenTelemetry, health checks, service discovery, and the standard `HttpClient` resilience handler. Aspire explicitly warns that ServiceDefaults is not a shared business-model project.
- Aspire supports NATS as a first-class resource. The integration supplies configuration, health checks, tracing, and JetStream durable storage.
- Aspire Dashboard receives OpenTelemetry logs, traces, and metrics and displays resource health. A separate logging/monitoring product is unnecessary for the local Version 1 foundation.
- Microsoft microservice guidance recommends service-owned data, avoiding long synchronous call chains, asynchronous integration events where eventual consistency is acceptable, and idempotent consumers with a transactional Outbox when publishing follows a database transaction.

## Project-fit reasoning

Aspire makes a distributed topology operable but does not make fine-grained services safe. Ledger, lots, portfolio cash, and financial calculations belong in one Portfolio Core service so one PostgreSQL transaction preserves financial invariants. Market Data, Slip Import, Research, and Operations have distinct data ownership, failure modes, dependencies, and lifecycle, so they are separate coarse-grained services.

Use HTTP/JSON for Gateway request/response paths. Keep synchronous depth to Gateway plus one backend service. Use NATS JetStream for durable background work and versioned cross-context notifications. Delivery is at-least-once; consumers require Inbox idempotency and publishers of business events require an Outbox in the same local database transaction. The foundation registers transport/health only and defines no business events.

## Accepted baseline

- ASP.NET Core/YARP Gateway as the only browser-facing backend.
- Aspire service discovery plus typed `HttpClient` and standard resilience for synchronous calls.
- NATS JetStream for asynchronous integration after business slices exist.
- One PostgreSQL server for local efficiency, with a separate database and `DbContext` per service.
- Aspire Dashboard as the only Version 1 technical observability UI.
- No gRPC until a benchmark proves a streaming or throughput need.

## Evidence

- [Aspire service discovery](https://aspire.dev/fundamentals/service-discovery/)
- [Aspire C# ServiceDefaults](https://aspire.dev/get-started/csharp-service-defaults/)
- [Aspire NATS integration](https://aspire.dev/integrations/messaging/nats/nats-get-started/)
- [Aspire Dashboard telemetry](https://aspire.dev/dashboard/apis/)
- [Microsoft data sovereignty per microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice)
- [Microsoft asynchronous communication guidance](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
- [Microsoft integration-event and idempotency guidance](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications)

