# Non-functional Requirements

## Correctness

- Use exact decimal arithmetic for money, quantity, prices, rates, fees, and taxes.
- Make rounding points explicit and testable; never silently round intermediate values for display convenience.
- Rebuild derived state deterministically from the confirmed ledger.

## Privacy and security

- Bind every Version 1 endpoint to loopback and reject unapproved hosts and origins.
- Keep runtime data, secrets, logs, and protection keys outside the repository.
- Redact sensitive fields and bound log retention.

## Reliability

- Make imports idempotent, corrections explicit, retries bounded, and provider failures non-destructive.
- Preserve the last known value with its stale status rather than presenting it as current.
- Verify restores, not only backup creation.

## Performance

- Establish measured budgets before optimization for startup, ledger rebuild, dashboard queries, market ingestion, and OCR.
- Keep interactive local reads responsive for the target dataset defined in the performance budget.

## Maintainability

- Keep domain/application layers provider-independent.
- Record external data provenance and parser/provider versions.
- Keep microservices coarse-grained, prohibit distributed financial transactions and cross-service database access, and avoid optional dependencies without evidence.
