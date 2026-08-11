# ADR-0014: Explicit Vertical-Slice Handlers Before a Mediator Library

## Status

Accepted for Version 1.

## Context

LedgerLens needs clear command/query use cases, strict module boundaries, and future cross-cutting behaviour without adding avoidable dependencies or indirection. The owner requires pragmatic Clean Code/SOLID and free libraries only, and asked whether a Mediator design is the right baseline.

## Decision

Use vertical slices and command-query separation with narrow `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>` contracts. Minimal API endpoints inject their exact handler. Do not add `IMediator`, MediatR, a runtime dispatcher, or service-location dispatch in the Version 1 baseline.

Use ASP.NET Core middleware/endpoint filters for HTTP cross-cutting concerns and explicit command-handler transaction boundaries. Reconsider a mediator package only after at least three implemented slices prove a common application pipeline need that is not cleanly served by framework boundaries. Any candidate must pass the free dependency and license gate. The MIT-licensed source-generated `Mediator` project is a candidate, not an admitted dependency.

All implementation follows the rules in [.NET Application Architecture](../architecture/DOTNET_APPLICATION_ARCHITECTURE.md).

## Alternatives considered

- MediatR from the first slice.
- The source-generated `Mediator` package from the first slice.
- A custom general-purpose mediator/dispatcher.
- Controller/service/repository horizontal layering without vertical slices.

## Evidence

- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [`Mediator` upstream repository and MIT license](https://github.com/martinothamar/Mediator)
- [Module boundaries](../architecture/MODULE_BOUNDARIES.md)

## Consequences

Use-case dependencies and call paths remain visible and easy to debug. There is no mediator pipeline at first, so repeated application-level behaviours must be measured before introducing one. Endpoint handlers must stay thin, and architecture tests must prevent business logic leaking to HTTP code.

## Risks

Developers may manually duplicate validation, transaction, or telemetry logic. Reviews must first place HTTP concerns in framework boundaries and domain rules in the model; genuine repeated application-pipeline logic triggers the revisit condition.

## Revisit conditions

Revisit when three or more implemented slices require the same ordered application pipeline, when in-process notification fan-out becomes a demonstrated requirement, or when direct handler injection creates measured maintenance problems.

