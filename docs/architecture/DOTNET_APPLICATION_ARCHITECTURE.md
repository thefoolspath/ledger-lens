# .NET Application Architecture and Design Rules

Status: Accepted for the distributed foundation. Last reviewed: 2026-09-02.

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

Business routes and their vertical-slice files follow the normative [API and Feature Naming Standard](API_AND_FEATURE_NAMING_STANDARD.md). The standard defines plural lowercase route features, result-cardinality operations, deterministic `OperationKey` derivation, one-public-type-per-file rules, and the planned Version 1 migration. Existing routes remain the implemented contract until [plan 0003](../plans/active/0003-api-feature-naming-migration.md) is executed and verified.

## Explicit handlers, not a mediator package

Future vertical slices use `ICommandHandler<TCommand,TResult>` and `IQueryHandler<TQuery,TResult>`, with the endpoint injecting its exact handler. There is no `IMediator`, runtime registry, MediatR, generic repository, mapper framework, or validation framework in the foundation. Middleware and endpoint filters own HTTP cross-cutting concerns.

## Communication

- Gateway routes HTTP/JSON through YARP and Aspire service discovery.
- Typed `HttpClient` uses standard resilience, explicit timeout and cancellation. Retry unsafe writes only with an idempotency contract.
- NATS JetStream carries versioned integration events/background commands after business logic exists. Delivery is at-least-once.
- `LedgerLens.IntegrationContracts` initially contains only a technical `MessageEnvelope<TPayload>`; it never contains domain entities.
- Event envelopes carry message identity, type, schema version, occurrence time, correlation and causation. Business ownership fields belong to the versioned payload when required.

### HTTP GET and QUERY policy

`GET` remains the default method for safe, idempotent resource retrieval when the request needs no content body. Use it for resource-by-ID routes, collections, and searches whose scalar filters, sort, and bounded pagination fit naturally in the URI query string. Prefer `GET` when the URL should be bookmarkable, shareable, directly navigable, or compatible with ordinary browser, proxy, cache, OpenAPI, and generated-client behaviour.

Use the HTTP `QUERY` method only for safe and idempotent reads that require structured request content, such as deeply nested filters, large identifier sets, multiple ranges, or a complex projection that is unsuitable for a bounded URI. A `QUERY` request carries its query description in the body with an explicit `Content-Type`; it must not create, mutate, confirm, reverse, or delete business state. Do not choose HTTP `QUERY` merely because the Application layer operation implements `IQueryHandler`; application queries and HTTP methods are separate concerns. Do not use it only to conceal sensitive data: request bodies still require the normal privacy, logging, authorization, and size controls.

Use `POST` for commands and state changes. When a safe structured read needs to support a client, Gateway, proxy, or tool that cannot carry `QUERY`, a documented `POST /.../search` compatibility endpoint is permitted, but its protocol semantics and retry/cache behaviour must not be presented as equivalent to `GET` or `QUERY`.

Before admitting a `QUERY` route, verify the complete Angular-to-Gateway-to-service path, including request-body forwarding, `Content-Type`, CORS/preflight where applicable, authorization/antiforgery behaviour, observability, OpenAPI representation, generated clients, and deployed intermediaries. ASP.NET Core 10 routes it with `MapMethods(pattern, [HttpMethods.Query], handler)` or `[AcceptVerbs("QUERY")]`; clients use `HttpMethod.Query`. Existing published routes keep their current verbs until a deliberate versioned contract migration and compatibility evidence are recorded.

References: [RFC 10008 — The HTTP QUERY Method](https://www.rfc-editor.org/rfc/rfc10008.html), [.NET 10 `HttpMethod.Query`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpmethod.query?view=net-10.0), and [ASP.NET Core 10 `HttpMethods.Query`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpmethods.query?view=aspnetcore-10.0).

## EF Core schema ownership

Each service has one scoped `DbContext` for its own database. Migrations are generated with pinned project-local `dotnet-ef`, inspected, tested against PostgreSQL, and applied through an explicit migration operation. Never use `EnsureCreated` for application databases or migrate silently during ordinary API startup.

Portfolio Core pilots the accepted hybrid workflow in ADR-0016. Its service-owned Database project contains generated persistence types and `DbContext`; Infrastructure alone maps them to Domain and implements Application ports; the Migrations project owns the snapshot and preserved migration chain. Reverse engineering is restricted to approved local/nonproduction sources and explicit service-owned tables. Generated code never enters Domain/Application, production is never a scaffold source, and migration history is never stamped without schema-equivalence evidence.

Future financial values use `decimal` and exact PostgreSQL `numeric`; queries use server-side filtering/projection, cancellation, bounded pagination, and one intentional materialization point.

LedgerLens-owned domain and persistent identifiers use UUIDv7 exclusively. Create new values with `Guid.CreateVersion7()` and reject non-v7 UUID values at API boundaries where the contract identifies a LedgerLens-owned resource. Do not replace external provider identifiers or OpenTelemetry trace identifiers with application UUIDs.

## Fixed local user and future OIDC

Identity remains business work for Phase 1. The foundation stores no personal email. Later, Gateway and services receive an `ICurrentUser` abstraction backed by fixed external configuration; Portfolio Core owns `UserProfile` and required Portfolio ownership. Future OIDC maps unique issuer/subject to the internal user rather than trusting email.

## Free dependency policy

Only dependencies with no fee for intended local and future production use may be admitted. Verify exact version, license, transitive/native components, maintenance, security, size, alternatives, and removal path. Trial, seat-based, metered, ambiguously dual-licensed, or source-available commercial packages are rejected by default.
