# ADR-0010: Runtime Data Outside the Repository

## Status

Accepted.

## Context

The repository is intended to become public while portfolio and document data are highly private.

## Decision

Store databases, slips, OCR artifacts, provider caches, logs, exports, backups, and protection keys only in an OS application-data directory or named volume outside the repository. Tracked examples are synthetic.

## Alternatives considered

Gitignored runtime folders inside the repository and encrypted real fixtures.

## Evidence

[Security architecture](../architecture/SECURITY_AND_PRIVACY.md) and [repository safety plan](../project/REPOSITORY_SAFETY_PLAN.md).

## Consequences

Accidental Git inclusion is less likely; setup, backup, and path discovery need explicit UX.

## Risks

Misconfiguration can still point into the repository.

## Revisit conditions

Never relax for real data; only refine platform-specific path selection and permissions.
