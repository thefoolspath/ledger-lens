# ADR-0011: Public Repository Safety

## Status

Accepted.

## Context

Ignore files alone do not prevent secret or personal-data history leaks.

## Decision

Use defence in depth: runtime boundary, accurate ignore rules, pre-commit and CI secret scanning, forbidden-path/file checks, GitHub secret scanning and push protection, CodeQL, dependency updates, branch protection, security policy, and private vulnerability reporting.

## Alternatives considered

`.gitignore` only, manual review only, and private repository indefinitely.

## Evidence

[Repository safety plan](../project/REPOSITORY_SAFETY_PLAN.md) and [GitHub secret scanning](https://docs.github.com/en/code-security/concepts/secret-security/about-alerts).

## Consequences

Leaks are less likely and detected earlier; contributors have additional checks and occasional false positives.

## Risks

Unsupported secret patterns and synthetic fixtures that resemble secrets.

## Revisit conditions

Review at repository initialization, provider addition, and each public release.
