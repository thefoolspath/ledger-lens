# ADR-0001: Aspire for Local Orchestration

## Status

Accepted.

## Context

LedgerLens needs a repeatable local graph for Angular, ASP.NET Core, PostgreSQL, optional workers, health, logs, and traces.

## Decision

Use a C# Aspire 13.4 stable AppHost on a project-local .NET 10 LTS SDK for development and local orchestration. Install the Aspire CLI into the short project-local `.a/` tool path and invoke SDK/CLI operations through repository wrappers without permanent `PATH` changes. The short directory is required because the NuGet tool layout exceeds Windows `MAX_PATH` under `.tools/aspire` at this repository location. Model Angular as a JavaScript application resource and PostgreSQL as an integration/container resource. Docker Desktop remains system-level. Aspire does not define the domain and is not described as a production runtime.

## Alternatives considered

Manual scripts, Docker Compose only, and multiple independently started projects.

## Evidence

[Platform evaluation](../research/PLATFORM_AND_DEPENDENCY_EVALUATION.md), [Aspire topology](../architecture/ASPIRE_TOPOLOGY.md), and [local development tooling](../project/LOCAL_DEVELOPMENT_TOOLING.md).

## Consequences

One resource graph improves local setup and observability. Project-local pinning makes setup reproducible and avoids global preview SDK drift, at the cost of bootstrap storage and maintenance.

## Risks

Frontend integration or Docker behaviour may change across versions.

## Revisit conditions

Revisit if the implementation spike cannot provide reliable Windows startup, health, or JavaScript integration.
