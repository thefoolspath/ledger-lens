# Release Strategy

Status: Proposed.

## Stages

- **Planning**: documentation and research gates; current stage.
- **Pre-alpha**: repository/tooling and synthetic vertical slices; no real-data recommendation.
- **Alpha 1**: manual portfolio core and verified backup/restore on owner machine.
- **Alpha 2**: market data, FX retrieval, freshness, and performance reporting.
- **Alpha 3**: Dime import only after its evidence gate.
- **Beta**: research center, migration/upgrade tests, broader hardware and accessibility checks.
- **1.0**: stable local data format, documented recovery, security gates, and supported upgrade path.

## Release evidence

Each release records scope, ADR changes, schema compatibility, verified commands, test/benchmark results, known risks, backup-before-upgrade instructions, rollback/restore procedure, and synthetic screenshots. Never publish provider data or a real portfolio.

No public package, repository, tag, or release is created without explicit owner approval. Version 1 remains loopback-only.
