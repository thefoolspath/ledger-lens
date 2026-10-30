# Access Administration and UI Resource Catalog

Last reviewed: 2026-09-23. Status: Proposed; implementation has not started.

## Purpose

This document completes the administration design for users, groups/roles, page/action permissions, UI resource keys, and English/Thai text. It extends [Identity, Access Control, and UI Localization Architecture](IDENTITY_ACCESS_AND_UI_LOCALIZATION.md) and ADR-0021.

The user-facing term may be translated as **Group** / **กลุ่มสิทธิ์**, while the domain, API, and database use the stable term `Role`. Membership is role-based by default. Per-user allow/deny overrides exist for exceptional cases and are always shown separately from inherited role grants.

## Source-of-truth boundaries

| Concern | Authoritative source | Reason |
| --- | --- | --- |
| Resource key, kind, parent page/menu, route, source feature, default English/Thai text | source-controlled UI resource manifest | code and endpoint contracts must not be invented or silently renamed in production |
| Typed frontend key constant | generated from the manifest | prevents scattered string literals and catches missing/renamed keys during build |
| Active/draft/retired catalog state | IdentityAccess database | supports review, publication, audit, and safe retirement |
| Role and user grants/denies | IdentityAccess database | runtime authorization policy managed by administrators |
| Administrator-edited translations | IdentityAccess database | versioned and auditable without rebuilding Angular |
| API error messages | existing RESX catalogs | remains separate from administrator-managed UI content |
| Runtime authorization decision | IdentityAccess effective-access calculation plus signed internal permission claims | browser state is never authoritative |

The synchronization process is one-way for definitions: manifest to database. Database role grants and administrator-edited translations never write back into source files.

## Manifest and frontend authoring contract

Resource keys are declared in one source-controlled manifest under the Angular core access boundary. The exact file format is implementation work, but the admitted shape is versioned JSON so .NET and Node tooling can consume it without reflection or a new parser dependency.

```json
{
  "schemaVersion": 1,
  "resources": [
    {
      "key": "portfolio",
      "kind": "Menu",
      "parentKey": null,
      "route": null,
      "sourceFeature": "portfolios",
      "texts": {
        "en-US": { "label": "Portfolio" },
        "th-TH": { "label": "พอร์ตการลงทุน" }
      }
    },
    {
      "key": "portfolio.accounts",
      "kind": "Page",
      "parentKey": "portfolio",
      "route": "/portfolios/:portfolioId/accounts",
      "sourceFeature": "investment-accounts",
      "texts": {
        "en-US": { "label": "Investment accounts" },
        "th-TH": { "label": "บัญชีลงทุน" }
      }
    },
    {
      "key": "portfolio.account.create",
      "kind": "Action",
      "parentKey": "portfolio.accounts",
      "route": null,
      "sourceFeature": "investment-accounts",
      "texts": {
        "en-US": { "label": "Create account" },
        "th-TH": { "label": "เพิ่มบัญชีลงทุน" }
      }
    }
  ]
}
```

A repository generator validates this manifest and produces a read-only `ui-resource-keys.generated.ts`. Angular code imports typed constants such as `UiResourceKeys.PortfolioAccountCreate`; templates and TypeScript do not repeat the raw string.

When a developer adds a button:

1. add the `Action` resource with both default languages and its parent page to the manifest;
2. run the generator, normally through the Angular validation/build workflow;
3. use the generated constant for visibility, label lookup, and the matching backend permission policy;
4. run catalog preview and review the diff;
5. apply the exact reviewed manifest hash to insert the resource as `Draft`;
6. grant it to roles, review translations, then publish it as `Active`.

A static architecture test rejects access-like raw literals outside the manifest, generated file, narrowly admitted API contracts, and tests. A new button whose key is absent from the manifest fails the build rather than becoming an ungoverned permission.

The sync tool does not scrape the built DOM, inspect a live browser, or import arbitrary matching strings. Those approaches cannot reliably determine parent page, resource kind, translations, or whether a string is a typo.

## Catalog lifecycle

Each database resource is in one of these states:

- `Draft`: imported but unavailable to ordinary roles; visible to System Administrator for review.
- `Active`: eligible for grants, navigation, page bootstrap, and endpoint policy checks.
- `RetirementPending`: absent from a newer manifest or explicitly proposed for retirement; existing grants remain visible and deny-safe.
- `Retired`: no longer emitted to clients or accepted as an active policy; audit history remains.

Sync never hard-deletes a resource, role grant, override, translation, or audit record. Removing a manifest entry marks it `RetirementPending`. Retirement requires an administrator review of affected roles/users and source/API usage.

A key rename is an explicit migration containing old key, new key, expected manifest hash, grant/translation movement, and compatibility/removal state. Delete-and-recreate is prohibited because it silently loses authorization history.

New resources are default-deny. The built-in `SystemAdministrator` role receives visibility to Draft resources and catalog administration so the system cannot be locked out during review. This role is protected by last-administrator and self-lockout rules.

## Administration information architecture

The Angular shell adds an `Administration` menu visible only with `administration.access.view`.

The sibling `Administration > Security` dashboard, detailed session views, MFA policy/enrollment administration, and recent step-up requirements are specified in [Session Visibility, Security Dashboard, and MFA Administration](SESSION_AND_MFA_ADMINISTRATION.md). “Active” means unrevoked/unexpired; “recently active” is a bounded `LastSeenAt` inference, not a claim of exact online presence.

### Users

The Users page provides bounded search/pagination and shows status, current email as profile data, linked provider summary without Google subject, assigned roles, override count, active-session count, and last successful login time.

The User detail page supports:

- enable/disable with reason;
- assign/remove roles;
- exceptional allow/deny overrides;
- effective-access preview showing `Role`, `User Allow`, `User Deny`, or `Default Deny` as the source;
- revoke one or all sessions;
- view privacy-safe security audit history.

Users that own financial records are disabled, never hard-deleted through this UI. Email cannot be edited into an identity link.

### Roles / Groups

The Roles page supports creating, renaming, describing, cloning, and archiving custom roles. System roles have immutable keys and protected invariants. An assigned role cannot be hard-deleted.

Role detail contains two coordinated tabs:

- **Members:** search users, add/remove membership, and show the resulting permission version.
- **Permissions:** hierarchical Menu → Page → Action/Capability tree with tri-state selection, text search, changed-only filter, and an effective-impact preview before save.

Saving the matrix is an atomic replacement guarded by an expected role version/ETag. It records added/removed keys, actor, correlation ID, reason, and timestamp without logging sensitive values.

### Access matrix

The cross-role matrix displays resources as rows and roles as columns for comparison. It is read-only initially; mutation stays in one role detail to keep concurrency and audit understandable. A later bulk editor requires its own measured use case.

### Resource catalog

The Resource Catalog page shows key, kind, parent, source feature, route, lifecycle state, manifest/database versions, role usage count, user override count, and translation completeness.

It provides:

- filter by `New`, `Changed`, `Missing from manifest`, `Draft`, `Active`, `RetirementPending`, or translation gap;
- inspect the last preview/apply diff and manifest hash;
- publish Draft resources;
- approve retirement only after all source and assignment checks pass;
- open the resource's translations and affected roles/users;
- view immutable sync/publish audit entries.

The production application does not upload or scan source code. It reviews catalog records already synchronized by the trusted developer/deployment operation.

### Translations

The Translation page edits `label`, `title`, `description`, and `tooltip` for supported cultures. It shows source default, current published value, draft value, fallback result, editor, and version. Publish is atomic per reviewed change set and invalidates the translation version/cache.

### Audit

The Access Audit page provides bounded filters for membership, role, override, catalog, translation, user-status, and session-revocation events. It stores identifiers and safe reason codes, not tokens, cookies, Google subjects, or raw request bodies.

## Administrative permission keys

Administration is protected by the same resource model it manages:

```text
administration.access.view
administration.users.view
administration.users.manage
administration.roles.view
administration.roles.manage
administration.resources.view
administration.resources.sync
administration.resources.publish
administration.translations.manage
administration.audit.view
administration.security-dashboard.view
administration.sessions.view
administration.sessions.revoke
administration.mfa.view
administration.mfa.reset
self.security.manage
```

System bootstrap seeds these immutable keys and the System Administrator role in the same migration/transaction. At least one active System Administrator must remain. A user cannot remove their own final administrator membership, deny their own administration access, disable the last administrator, or archive the last role that preserves recovery.

## Proposed API operations

Routes follow the repository's Version 1 feature/operation naming rules and use `{ data, meta }` / Problem Details.

```text
GET  /api/access/v1/users/get-page
GET  /api/access/v1/users/get-one/{userId}
POST /api/access/v1/users/update-status/{userId}
POST /api/access/v1/users/roles/assign
POST /api/access/v1/users/roles/remove
POST /api/access/v1/users/resources/replace-overrides/{userId}
POST /api/access/v1/users/sessions/revoke-list/{userId}

GET  /api/access/v1/security-dashboard/get-one
GET  /api/access/v1/user-sessions/get-page
GET  /api/access/v1/users/sessions/get-list/{userId}
POST /api/access/v1/user-sessions/revoke/{sessionId}
POST /api/access/v1/users/mfa/update-requirement/{userId}
POST /api/access/v1/users/mfa/reset/{userId}

GET  /api/access/v1/roles/get-page
GET  /api/access/v1/roles/get-one/{roleId}
POST /api/access/v1/roles/create
POST /api/access/v1/roles/update/{roleId}
POST /api/access/v1/roles/archive/{roleId}
GET  /api/access/v1/roles/resources/get-tree/{roleId}
POST /api/access/v1/roles/resources/replace/{roleId}

GET  /api/access/v1/access-resources/get-page
GET  /api/access/v1/access-resources/get-one/{resourceKey}
POST /api/access/v1/access-resources/publish-list
POST /api/access/v1/access-resources/retire-list
GET  /api/access/v1/access-audit-events/get-page

GET  /api/access/v1/navigation/get-list
GET  /api/access/v1/page-bootstrap/get-one/{pageKey}
```

Catalog ingestion is not a normal browser business API. A private operation is callable only by the local Aspire command/deployment identity, with a service-audience token and `administration.resources.sync`. Its preview is read-only; apply requires the exact previewed manifest hash.

## Catalog synchronization workflow

### Preview

The sync runner validates schema version, key naming, uniqueness, parent existence, acyclic hierarchy, allowed kinds, required English/Thai defaults, known routes/features, and manifest hash. It compares the manifest to the database and returns:

- additions;
- metadata/default-text changes;
- missing/retirement candidates;
- invalid renames or parent/kind changes;
- affected roles/users/translations;
- blocking conflicts.

Preview performs no database mutation and returns a bounded Markdown/JSON result suitable for the Aspire Dashboard.

### Apply

Apply accepts the exact previewed hash and an explicit confirmation. In one IdentityAccess database transaction it:

1. rejects a changed/stale manifest or any blocking conflict;
2. inserts new resources as `Draft`;
3. updates safe metadata;
4. updates source defaults without overwriting an administrator-published translation;
5. marks missing entries `RetirementPending`;
6. increments catalog/permission/translation versions as applicable;
7. writes one sync audit record plus bounded item summaries;
8. invalidates affected caches after commit.

Re-running the same manifest is idempotent and reports no changes.

## Aspire Dashboard integration

Aspire officially supports custom resource commands displayed under a resource's Actions menu and invokable through `aspire resource`. This is the recommended local developer surface; no custom Dashboard plugin or public admin endpoint is needed.

Attach these local-only commands to the proposed `identity-catalog-sync` resource:

- `preview-ui-resource-catalog`: validates and returns the diff plus manifest hash in Markdown/JSON.
- `apply-ui-resource-catalog`: requires the preview hash and an explicit confirmation input, shows progress, and returns the committed version/audit identifier.

The commands are visible only in the local Dashboard UI/approved CLI mode, enabled only in Development, and operate through the same IdentityAccess application handlers as deployment automation. They must not print manifest secrets, credentials, or PII. The exact `WithCommand`/resource-command API must be verified against pinned Aspire 13.4.6 before implementation.

Production catalog synchronization is an authenticated deployment/maintenance step using the same runner and reviewed manifest artifact. Aspire Dashboard is never exposed merely to obtain the buttons.

Official reference: [Aspire custom resource commands](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/custom-resource-commands).

## Concurrency, cache, and permission invalidation

- Every role, override set, catalog, and translation change uses optimistic concurrency/version checks.
- Membership/grant/deny/status changes increment the user's or global permission version.
- A new page-bootstrap or internal token observes the new version immediately; an already issued internal token expires within its short lifetime.
- Disabling a user or applying an explicit deny revokes sessions or bypasses the ordinary cache so the bounded token lifetime is not the only control.
- Catalog/translation publication invalidates only affected page/culture cache entries when possible.
- A sync or administration failure is fail-closed for new grants and does not remove existing financial ownership.

## Security and abuse cases

- A malicious manifest cannot grant itself authority: new resources are Draft/default-deny and require a separately authorized publish/grant operation.
- A developer typo cannot silently become a live permission because validation/build fails or sync blocks.
- A compromised ordinary administrator cannot modify system-role invariants or the last-administrator recovery path.
- A stale tab cannot overwrite a newer role matrix because expected versions are mandatory.
- A browser cannot invoke catalog ingestion merely by knowing its route; only the local/deployment service identity is admitted.
- A missing resource or unavailable catalog never converts a deny to allow.
- Translation content is encoded as text, length-limited, and cannot contain executable HTML by default.

## Acceptance tests

- manifest schema, duplicate, orphan parent, cycle, invalid kind/key, missing language, rename, and hash mismatch tests;
- generator determinism and static rejection of unregistered raw access keys;
- preview has zero writes; apply is transactional/idempotent and rejects stale hashes;
- new key becomes Draft/default-deny and is visible to System Administrator review;
- removal becomes RetirementPending without losing grants/audit;
- administrator translation survives default-text refresh;
- role create/clone/archive, membership assignment, effective-access source, override precedence, and atomic matrix replacement;
- last-admin/self-lockout prevention and disabled-user/session invalidation;
- permission/translation version and cache invalidation behavior;
- Aspire commands are absent outside Development and do not expose sensitive output;
- Angular menu/page/button behavior matches server enforcement, including direct forbidden requests;
- accessibility and keyboard operation for tree/matrix/dialogs, plus large-role/resource pagination and performance bounds.
