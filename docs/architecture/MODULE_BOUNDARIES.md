# Service Boundaries

Status: Accepted and partially implemented. Last reviewed: 2026-09-24.

| Service | Owns | May communicate with |
| --- | --- | --- |
| Gateway | implemented public routes and HTTP composition; proposed OIDC/BFF | all current service HTTP APIs; proposed IdentityAccess |
| IdentityAccess (proposed) | local users, external identity mappings, sessions, roles, resource grants, UI resource catalog/localized text, security audit | Gateway HTTP; service-owned identity contracts only |
| Portfolio Core | implemented local user projection, portfolios, accounts, cash ledger, and isolated Simulation; later confirmed trades/lots/calculations remain | NATS; Gateway HTTP |
| Market Data | implemented provider abstraction, quotes, candles, FX, freshness/quota results; later external-provider/event coverage remains | NATS; Gateway HTTP |
| Slip Import | foundation host now; documents, hashes, extraction, review and posting state remain proposed | NATS; Gateway HTTP |
| Research | foundation host now; watchlists, notes, theses, sources and comparable research remain proposed | NATS; Gateway HTTP |
| Operations | foundation host now; backup/restore orchestration, manifests and maintenance state remain proposed | NATS; service-owned operational contracts only |

## Boundary rules

- Each service owns a separate database, `DbContext`, EF mappings, migrations, API contracts, and domain language. A service may contain its own generated Database project, but that project is persistence-only and is visible only to the owning Infrastructure and Migrations projects.
- No service references another service's Domain, Application, Infrastructure, EF entity, migration, or database project.
- Domain, Application, and API projects never reference a Database project directly; generated persistence entities never become transport or domain contracts.
- Shared projects are limited to ServiceDefaults and versioned technical integration contracts.
- Synchronous request depth is Gateway plus one backend. Avoid backend-to-backend HTTP chains.
- NATS delivery is at-least-once. Business publishers require a transactional Outbox and consumers require Inbox idempotency when business events are introduced.
- Breaking HTTP/event contracts create a new version; do not mutate an established semantic contract.
- Provider and OCR DTOs remain private to their Infrastructure layer.
- If ADR-0019 is accepted, IdentityAccess becomes the identity/access authority while Portfolio Core retains its local `UserProfile` projection and financial ownership. No service reads the identity database directly, and email/provider claims do not become cross-service identity keys.
