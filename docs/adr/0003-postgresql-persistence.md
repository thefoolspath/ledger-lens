# ADR-0003: PostgreSQL Persistence

## Status

Accepted for Version 1.

## Context

LedgerLens needs exact numeric types, relational constraints, transactions, time-series indexing, backup tools, and EF Core integration.

## Decision

Use one PostgreSQL 18.x server for the local topology with a separate service-owned database, `DbContext`, credentials and migration history per microservice. Use EF Core/Npgsql 10.x, exact `numeric` values, UTC `timestamptz` instants plus explicit exchange-local context, and prohibit cross-service database access. Store document binaries outside the database.

## Alternatives considered

SQLite, SQL Server, document database, and one database per module.

## Evidence

[Platform evaluation](../research/PLATFORM_AND_DEPENDENCY_EVALUATION.md) and [data model](../architecture/DATA_MODEL.md).

## Consequences

Strong relational/rebuild guarantees and mature backup tooling; Docker/local database operation adds startup and storage management.

## Risks

Premature precision/index choices or EF translation surprises.

## Revisit conditions

Revisit numeric scales and indexes after representative fixtures and query benchmarks; database choice only if the local operational burden proves unacceptable.
