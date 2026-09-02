# Service Boundaries

Status: Accepted foundation direction.

| Service | Owns | May communicate with |
| --- | --- | --- |
| Gateway | public routes, HTTP composition, future OIDC/BFF | all service HTTP APIs |
| Portfolio Core | future users, portfolios, accounts, ledger, lots, cash, calculations | NATS; Gateway HTTP only in foundation |
| Market Data | future provider adapters, quotes, candles, FX, freshness and quota | NATS; Gateway HTTP only in foundation |
| Slip Import | future documents, hashes, extraction, review and posting state | NATS; Gateway HTTP only in foundation |
| Research | future watchlists, notes, theses, sources and comparable research | NATS; Gateway HTTP only in foundation |
| Operations | future backup/restore orchestration, manifests and maintenance state | NATS; service-owned operational contracts only |

## Boundary rules

- Each service owns a separate database, `DbContext`, EF mappings, migrations, API contracts, and domain language. A service may contain its own generated Database project, but that project is persistence-only and is visible only to the owning Infrastructure and Migrations projects.
- No service references another service's Domain, Application, Infrastructure, EF entity, migration, or database project.
- Domain, Application, and API projects never reference a Database project directly; generated persistence entities never become transport or domain contracts.
- Shared projects are limited to ServiceDefaults and versioned technical integration contracts.
- Synchronous request depth is Gateway plus one backend. Avoid backend-to-backend HTTP chains.
- NATS delivery is at-least-once. Business publishers require a transactional Outbox and consumers require Inbox idempotency when business events are introduced.
- Breaking HTTP/event contracts create a new version; do not mutate an established semantic contract.
- Provider and OCR DTOs remain private to their Infrastructure layer.
