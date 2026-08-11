# ADR-0012: Defer Authentication and Define a Future OIDC Gate

## Status

Deferred for Version 1.

## Context

The owner wants the fastest trustworthy local workflow and does not need network access in Version 1.

## Decision

Do not implement login, Identity, password, JWT, role, or user administration in Version 1. Bind strictly to loopback.

Introduce an explicit `ICurrentUser` boundary with a `FixedLocalCurrentUser` implementation. Persist `UserProfile.Id` and require `Portfolio.OwnerUserId`; descendants inherit ownership through Portfolio rather than copying nullable `UserId` into every table. The stable local identifier and owner email are supplied through User Secrets or external runtime configuration and bootstrapped idempotently, never hard-coded into tracked source, migrations, logs, or fixtures.

Before external access, replace the fixed provider with a claims-backed provider and require OIDC sign-in, OAuth where delegated API access is needed, unique issuer/subject mapping to the local user, portfolio authorization, HTTPS, CSRF controls, rate limiting, audit logging, secret hardening, and encrypted backup. Email is profile data and is not a durable external identity key.

## Alternatives considered

ASP.NET Core Identity now, OAuth/OIDC now, a personal email constant in source, using email as a relational key, and nullable `UserId` on every table.

## Evidence

[Security architecture](../architecture/SECURITY_AND_PRIVACY.md) and [localhost evaluation](../research/LOCALHOST_SECURITY_EVALUATION.md).

## Consequences

MVP scope stays small while ownership is testable and future identity mapping does not require rewriting domain contracts. Local setup needs one external fixed-user configuration and an idempotent bootstrap. Any accidental non-loopback exposure remains critical.

## Risks

Developers may assume localhost is automatically safe.

## Revisit conditions

Revisit only when remote/LAN/multi-user access becomes an accepted product requirement.
