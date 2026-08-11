# System Overview

Status: Accepted foundation direction. Last reviewed: 2026-08-11.

LedgerLens is a loopback-only distributed application orchestrated by Aspire.

```text
Angular -> ASP.NET Core/YARP Gateway
              |-> Portfolio Core -> portfolio database
              |-> Market Data    -> market database
              |-> Slip Import    -> slip database / document store / OCR worker
              |-> Research       -> research database
              `-> Operations     -> operations database

Services <-> NATS JetStream
All .NET resources -> Aspire Dashboard via OpenTelemetry
```

## Trust and consistency boundaries

1. Browser calls only Gateway; browser input remains untrusted despite loopback.
2. Each service owns its process, domain model, database, migrations, and external adapters.
3. Portfolio Core is the atomic financial boundary for portfolio, ledger, lots, cash, and calculations.
4. HTTP contracts and NATS integration events are untrusted/versioned boundaries; no cross-service entity or `DbContext` sharing is allowed.
5. Repository and runtime storage are separate; databases, JetStream, documents, logs, secrets, and backups remain outside Git.

## Architectural invariants

- Follow the [Clean Architecture rules](DOTNET_APPLICATION_ARCHITECTURE.md) independently inside each service.
- Angular never receives provider secrets, NATS credentials, internal service URLs, or database access.
- Gateway is the only browser-facing backend endpoint.
- Ledger entries are authoritative; holdings and snapshots are rebuildable Portfolio Core projections.
- Do not split a financial transaction across services or introduce a distributed transaction.
- Use technical logs for operations only; business audit evidence remains domain data.
- Aspire models, runs, connects, and observes the application; it does not replace security or justify fine-grained services.

See [ADR-0015](../adr/0015-coarse-grained-microservices.md).

