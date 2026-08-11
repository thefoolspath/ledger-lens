# Backup and Restore

Status: Proposed.

## Backup unit

A backup is a coordinated set containing a PostgreSQL logical dump, original slip documents, required derived artifacts, and a manifest with schema/app version, creation instant, file size, SHA-256, and relative identity. It excludes API keys and transient logs.

## Procedure

1. Enter a short maintenance/read-consistent state.
2. Create a PostgreSQL logical dump using a supported tool matching the server major version.
3. copy immutable documents referenced by the captured database state;
4. write and validate the manifest and checksums;
5. optionally encrypt the completed archive with a separately managed key;
6. retain daily/weekly copies under a documented size budget.

## Restore

Restore only into an empty target, verify every checksum, restore the database, restore documents, run migrations only under an explicit compatibility procedure, validate referential/document integrity, rebuild projections, and compare reconciliation totals.

At least one automated clean-environment restore test is required before MVP release. A backup is not considered successful until restoration has been verified.

Reference: [PostgreSQL backup and restore](https://www.postgresql.org/docs/current/backup.html).
