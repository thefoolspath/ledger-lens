# .NET Application Architecture and Design Rules

Status: Accepted for the distributed foundation. Last reviewed: 2026-08-11.

## Non-negotiable rules

All production code and tests follow pragmatic Clean Code, SOLID, DRY, KISS, YAGNI, high cohesion, low coupling, explicit dependencies, deterministic behaviour, and framework-independent business policy. A pattern or dependency must solve a demonstrated problem.

Prohibited without recorded evidence: generic repositories, extra unit-of-work wrappers over EF Core, `BaseService`/`BaseController` hierarchies, service location through `IServiceProvider`, domain entities as transport contracts, business logic in endpoints, cross-service project/database references, binary floating point for canonical finance, hidden writes, `SaveChanges` in loops, unbounded queries, or distributed financial transactions.

## Per-service Clean Architecture

Each business service is independently structured as:

```text
Service.Domain <- Service.Application <- Service.Infrastructure <- Service.Api/Worker
```

- **Domain**: future entities, value objects, invariants, and domain services; no HTTP, EF, logging, NATS, or provider dependency.
- **Application**: commands, queries, explicit handlers, results, and narrow ports; no API or provider DTOs.
- **Infrastructure**: EF Core/Npgsql, external adapters, NATS/Outbox implementations, and configuration.
- **Api/Worker**: composition root, transport mapping, authorization boundary, and hosting only.

Architecture tests enforce direction within a service and prevent references across service internals. No common Domain/SharedKernel project is created. Concepts that happen to share names may evolve independently in different bounded contexts.

## Explicit handlers, not a mediator package

Future vertical slices use `ICommandHandler<TCommand,TResult>` and `IQueryHandler<TQuery,TResult>`, with the endpoint injecting its exact handler. There is no `IMediator`, runtime registry, MediatR, generic repository, mapper framework, or validation framework in the foundation. Middleware and endpoint filters own HTTP cross-cutting concerns.

## Communication

- Gateway routes HTTP/JSON through YARP and Aspire service discovery.
- Typed `HttpClient` uses standard resilience, explicit timeout and cancellation. Retry unsafe writes only with an idempotency contract.
- NATS JetStream carries versioned integration events/background commands after business logic exists. Delivery is at-least-once.
- `LedgerLens.IntegrationContracts` initially contains only a technical `MessageEnvelope<TPayload>`; it never contains domain entities.
- Event envelopes carry message identity, type, schema version, occurrence time, correlation and causation. Business ownership fields belong to the versioned payload when required.

## EF Core Code First

Each service has one scoped `DbContext` and design-time factory for its own database. Migrations are generated with pinned project-local `dotnet-ef`, inspected, tested against PostgreSQL, and applied through an explicit migration operation. Never use `EnsureCreated` for application databases or migrate silently during ordinary API startup. The foundation contains no business entities or tables.

Future financial values use `decimal` and exact PostgreSQL `numeric`; queries use server-side filtering/projection, cancellation, bounded pagination, and one intentional materialization point.

LedgerLens-owned domain and persistent identifiers use UUIDv7 exclusively. Create new values with `Guid.CreateVersion7()` and reject non-v7 UUID values at API boundaries where the contract identifies a LedgerLens-owned resource. Do not replace external provider identifiers or OpenTelemetry trace identifiers with application UUIDs.

## Fixed local user and future OIDC

Identity remains business work for Phase 1. The foundation stores no personal email. Later, Gateway and services receive an `ICurrentUser` abstraction backed by fixed external configuration; Portfolio Core owns `UserProfile` and required Portfolio ownership. Future OIDC maps unique issuer/subject to the internal user rather than trusting email.

## Free dependency policy

Only dependencies with no fee for intended local and future production use may be admitted. Verify exact version, license, transitive/native components, maintenance, security, size, alternatives, and removal path. Trial, seat-based, metered, ambiguously dual-licensed, or source-available commercial packages are rejected by default.
