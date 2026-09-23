# ADR-0021: Shared Resource Keys for Access Control and UI Localization

## Status

Proposed; owner confirmation and implementation evidence are pending.

## Context

The owner requires per-user control over visible menus, accessible pages, and enabled buttons/actions. The key used to authorize a button must also identify its localized UI label. Entering a page should retrieve that page's effective permission and language content immediately.

Existing RESX resources localize API error messages. They are not an administrator-managed navigation/page/button catalog, and plan 0004 requires a separate ADR before such database-backed content is admitted.

## Proposed decision

Introduce immutable `ResourceKey` values for `Menu`, `Page`, `Action`, and nonvisual `Capability` resources. An action key is shared by the role/user grant, backend authorization policy, Angular action check, and localized label. Display-only copy uses related translation-only keys and does not become a permission.

Definitions originate in a versioned, source-controlled manifest that generates typed Angular constants. Raw access-key literals outside the manifest/generated boundary are rejected. A validated synchronization runner previews and applies the manifest to IdentityAccess; it never scrapes a rendered DOM or imports arbitrary matching strings.

IdentityAccess owns roles, resource grants, user overrides, user culture, resource topology, and versioned localized texts. Authorization is default-deny with disabled user, user deny, user allow, role grant, then default deny precedence.

Angular loads authorized navigation after session bootstrap and an atomic page-bootstrap response before activating a feature route. The page response combines `canAccess`, action keys, resolved texts, permission version, and translation version. Backend endpoint authorization and data ownership remain authoritative; hiding a control is only user experience.

English and Thai source-controlled defaults remain available. Database edits require authorization, audit, versioned publish, cache invalidation, English fallback, and safe missing-key behavior. This catalog does not replace Problem Details RESX resources.

New resources enter as `Draft` and default-deny. Missing manifest resources become `RetirementPending`; sync never hard-deletes grants, overrides, translations, or audit history. Resource renames require an explicit migration.

Use local-only Aspire custom resource commands for manifest preview/apply and the LedgerLens Administration UI for user status, role/group membership, permission matrices, per-user overrides, resource publication/retirement, translations, sessions, and audit. The detailed design is [Access Administration and UI Resource Catalog](../architecture/ACCESS_ADMINISTRATION_AND_RESOURCE_CATALOG.md).

## Alternatives considered

- separate permission and translation keys: rejected because it permits drift between an action and its label;
- translating text directly in Angular templates: rejected because page bootstrap and administrator management require a central contract;
- treating every text key as a permission: rejected because headings and explanatory copy are not capabilities;
- trusting route guards/button hiding: rejected because browser code is not an authorization boundary.
- scanning frontend source/DOM for string patterns: rejected because it cannot reliably determine kind, hierarchy, translation metadata, or typo intent;
- making the Aspire Dashboard a production administration surface: rejected because its commands are local developer/deployment operations, not product authorization UI.

## Consequences

Resource keys become stable public contracts and require naming/static tests. Database-managed content adds availability, fallback, audit, and cache responsibilities. The combined page response avoids mismatched permission/text versions but couples page composition to IdentityAccess availability.

## Evidence gate

Accept only after key naming, manifest/generator rules, preview/apply lifecycle, seed/publish workflow, role/group administration, override precedence, lockout prevention, translation fallback, cache invalidation, endpoint-policy mapping, Aspire command compatibility, and Angular page-bootstrap tests are decision-complete.
