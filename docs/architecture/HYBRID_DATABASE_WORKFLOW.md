# Portfolio Core Hybrid Database Workflow

Status: Implemented pilot. Last reviewed: 2026-09-01.

## Ownership and dependency direction

```text
PortfolioCore.Api
  -> PortfolioCore.Infrastructure
       -> PortfolioCore.Database

PortfolioCore.Migrations
  -> PortfolioCore.Database
```

`PortfolioCore.Database` contains generated persistence classes and the generated-style `PortfolioCoreDbContext`. Infrastructure maps these records to the existing domain types and implements the Application ports. Domain and Application remain independent of EF Core. The migrations executable owns migrations and the snapshot and preserves the existing migration IDs.

This is a class-library boundary, not a network database service. It adds no request hop and requires no NuGet publication.

## Commands

Use the repository-local wrapper and provide the connection only in the current process or an external secret mechanism:

```powershell
Set-Item -Path 'Env:ConnectionStrings__portfolio-db' -Value '<PostgreSQL connection string>'
lg db scaffold
lg db migration AddReviewedSchemaChange
lg db check
lg db script <applied-migration> <target-migration> [<output-path>]
```

For a non-local approved nonproduction source, also set the exact host:

```powershell
Set-Item -Path 'Env:LEDGERLENS_DB_SYNC_APPROVED_HOST' -Value '<exact-nonproduction-host>'
```

`scaffold` reads only `user_profiles`, `portfolios`, `investment_accounts`, `cash_ledger_entries`, `simulation_accounts`, `simulation_trade_drafts`, `simulation_trade_entries`, and `simulation_valuation_snapshots`; changing the service-owned table set requires a reviewed script update. It does not scaffold `__EFMigrationsHistory`. Generated output is limited to the Database project and uses `--no-onconfiguring` so a connection string is never emitted into source.

`migration` creates a migration in the Migrations project and stops for manual review if it finds `DropTable`, `DropColumn`, `AlterColumn`, or raw SQL. `check` fails when the current EF model differs from the committed snapshot. `script` writes ignored review output under `artifacts/database/portfolio-core/` by default.

## Required review gate

Before a migration can be deployed or an already-changed source database can be marked at that migration:

1. Review the generated persistence diff and confirm Domain/Application files were untouched.
2. Review the migration and SQL for renames represented as drop/add, type narrowing, default changes, lost indexes or constraints, and data backfills.
3. Apply the complete migration chain to an empty ephemeral PostgreSQL database.
4. Apply the new migration from a fixture at the previous migration and verify preserved synthetic rows and reconciliation invariants.
5. Compare the resulting PostgreSQL tables, columns, types, defaults, indexes, foreign keys, unique constraints, and sequences with the approved source schema.
6. Use a separately authorized deployment identity for the target environment.

The repository command intentionally has no `apply`, `database update`, or history-stamp operation. Production uses a reviewed SQL artifact or separately approved migration bundle. Production is never scaffolded and ordinary API replicas never run migrations.
