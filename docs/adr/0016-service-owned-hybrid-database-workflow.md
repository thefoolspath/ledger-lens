# ADR-0016: Service-Owned Hybrid Database Workflow

Status: Accepted for Portfolio Core pilot. Date: 2026-09-01.

## Context

LedgerLens originally treated the EF Core model and Code First migrations as the only schema-authoring source. The owner requires a controlled hybrid workflow similar to the WMS database libraries: an approved local or nonproduction PostgreSQL schema may be reverse engineered, while reviewed EF migrations remain the deployment record for other environments.

Repeated scaffolding must not overwrite domain behaviour, expose a `DbContext` across service boundaries, or make an EF history row authoritative without evidence that the schema and migration model agree.

## Decision

Pilot the workflow in Portfolio Core with these boundaries:

- `LedgerLens.PortfolioCore.Database` is a service-owned class library containing only the generated persistence entities and `PortfolioCoreDbContext`.
- Infrastructure references the Database project and maps between persistence entities and domain entities. Domain, Application, and API do not reference the Database project directly.
- `LedgerLens.PortfolioCore.Migrations` owns the preserved migration IDs, model snapshot, design-time factory, and one-shot migration executable. Ordinary API startup never applies migrations.
- The Database and Migrations projects use `ProjectReference`; publishing a NuGet package is not required. A package may be introduced later only if an independently released consumer is demonstrated.
- `lg db scaffold` uses the pinned repository-local EF tool, reads only the explicit Portfolio Core table allowlist, suppresses connection-string generation, and overwrites only generated Database-project files.
- Non-local scaffolding requires an exact `LEDGERLENS_DB_SYNC_APPROVED_HOST` match. Production databases are never valid scaffold sources.
- Migration creation fails closed for `DropTable`, `DropColumn`, `AlterColumn`, and raw SQL until a human reviews and corrects the generated migration.
- The repository tool generates reviewable SQL and checks model-snapshot drift, but it never applies migrations and never inserts `__EFMigrationsHistory` rows.
- A history row may be recorded by a separately approved deployment process only after the migration has been tested from the prior schema and schema equivalence has been demonstrated. The history row alone is not evidence of equivalence.
- The application runtime identity should not receive DDL permission outside local development; scaffold, migration, and runtime identities are separate deployment concerns.

## Consequences

The generated database representation can follow DBA-authored schema changes without coupling Domain to generated code. Project references preserve local debugging and atomic repository changes without a package feed. Infrastructure now owns explicit mapping code, and schema changes require additional review, migration, and equivalence evidence. Portfolio Core is the only pilot; extension to another service requires that service's own Database project, tests, and documented acceptance evidence.
