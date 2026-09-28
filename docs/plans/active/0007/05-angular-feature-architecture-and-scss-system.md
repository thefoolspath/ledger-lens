# 0007.05 — Restructure Angular by business feature, then establish a global SCSS design system

Last reviewed: 2026-09-28.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: Frontend structure and visual system

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [0007.03](03-semantic-naming-consistency.md) — Angular types, functions, folders, and shared concepts use the governed vocabulary.
- [0007.04](04-collection-correctness-pagination-and-sorting.md) — Simulation pagination follows behavior-preserving feature extraction and owns route/query state.
- [0007.06](06-request-lifecycle-timeouts-mutations-and-exports.md) — feature data-access streams own superseded-request and destruction cancellation.
- Milestone 5B in this file depends on verified Milestone 5A usage; styling primitives must not be designed before real extracted-feature consumers exist.


**Status:** Accepted as two separately verified milestones; implementation not started.

### Current behavior

The Angular application is a verified functional vertical shell, but its source is not yet organized for continued feature growth. `src/app/app.ts` currently owns most transport models, page state, request orchestration, formatting helpers, Portfolio workflows, Cash Ledger workflows, Market Data workflows, and Simulation workflows. `app.html` similarly renders the main real-portfolio and simulation experiences, while `app.routes.ts` contains no application routes. The extracted `api-routes.ts`, `api-response.ts`, and `simulation-chart.ts` files are useful first boundaries but do not yet form feature-owned frontend slices.

Global styling is currently split between `styles.css`, component CSS, and Tailwind utility usage. This does not guarantee that tables, selects, autocomplete controls, form fields, buttons, loading states, and error states share one visual contract or that changing one global definition updates every page. Page-specific customization also needs a supported override mechanism that does not depend on deep selectors, repeated hard-coded values, or uncontrolled `!important` rules.

### Milestone 5A — accepted feature architecture

Restructure the Angular application as a feature-first, workflow-oriented standalone application. The root application becomes a thin shell containing global layout, navigation, notification/error presentation, and `router-outlet`. Business behavior is owned by feature routes and cohesive feature-local boundaries. The primary landing route may remain eager when that gives the best first-navigation experience; other substantial workflows are lazy by default and remain subject to production bundle/chunk evidence. Do not add nested lazy boundaries merely for structural symmetry.

The initial top-level ownership is:

```text
src/app/
|-- app.ts
|-- app.html
|-- app.config.ts
|-- app.routes.ts
|-- core/
|   |-- api/
|   |-- configuration/
|   `-- layout/
|-- shared/
|   |-- formatting/
|   |-- models/
|   `-- ui/
`-- features/
    |-- portfolios/
    |-- investment-accounts/
    |-- cash-ledger/
    |-- simulation/
    `-- market-data/
```

Use `cash-ledger` rather than the ambiguous `ledger` feature name so it remains distinct from the LedgerLens product name and leaves room for stock-ledger, FX-cash-lot, and reconciliation capabilities. Frontend folders follow user workflows and cohesive UI ownership; they do not mirror every backend operation folder mechanically.

Keep a small feature's component, template, styles, API client/model, and tests colocated by workflow. Split further by cohesive subfeature when the directory becomes difficult to navigate; do not create type-based folders merely to satisfy a template. An expanded feature may use a shape such as:

```text
features/<feature>/
|-- <feature>-page.ts
|-- <feature>-page.html
|-- <feature>-page.scss
|-- <feature>.api.ts
|-- <feature>.models.ts
|-- <feature>.spec.ts
|-- <cohesive-subfeature>/
`-- <feature>.routes.ts
```

- Pages read route state, coordinate the user workflow, and connect feature state to presentational components.
- Components use explicit inputs and outputs and do not know API URLs.
- Data-access services own typed HTTP communication and success-envelope unwrapping.
- Prefer component-local signals for local state. Introduce a feature store only when state is shared across multiple components, must outlive one component within the route, or has enough asynchronous transitions to benefit from an explicit state owner; provide it at the narrowest appropriate route/feature boundary.
- Models remain feature-owned unless they are genuinely shared by multiple features.
- NgRx is not introduced without measured state complexity that cannot be handled clearly by route-scoped signal stores.
- Root or cross-feature services must not become a new all-purpose replacement for the current root component.
- Features may depend on `core` and genuinely shared primitives; `core`/`shared` must not depend on a business feature. Cross-feature imports require an explicit public boundary or relocation of the genuinely shared concept.
- Route/query state owns bookmarkable resource IDs, filters, page, and sort. Per-operation loading/failure state replaces the current single global `busy`/`error` state where concurrent work can occur.
- Superseded HTTP requests are cancelled and stale responses cannot overwrite newer selections, pages, sorts, or filters. Expected cancellation does not create a user-visible error.
- Preserve direct ECharts lazy loading, lifecycle disposal, ARIA/table fallbacks, and the current initial-bundle budget when extracting Simulation.

### Milestone 5B — accepted global styling system

SCSS is the source of truth for application styling. Convert the global and component style entry points from CSS to SCSS and organize the global design system under `src/styles/`:

```text
src/
|-- styles.scss
`-- styles/
    |-- _tokens.scss
    |-- _themes.scss
    |-- _reset.scss
    |-- _typography.scss
    |-- _layout.scss
    |-- _forms.scss
    |-- _tables.scss
    |-- _buttons.scss
    |-- _overlays.scss
    `-- _utilities.scss
```

`styles.scss` is the single global entry point and composes the partials with Sass modules. SCSS organizes and generates the styles, while globally named CSS custom properties under `:root` provide runtime design tokens for colors, typography, spacing, control sizes, table density, borders, radii, shadows, focus indicators, and layering. Changing an admitted global token or shared component rule must update every consuming page without editing feature templates or feature styles.

Shared controls are implemented under `src/app/shared/ui/` and use an `ll-` selector/class prefix. Admit a component/directive only after at least two real consumers demonstrate shared behavior/accessibility, or when a single complex control needs one controlled integration boundary. Do not wrap every raw input, select, button, or table speculatively. The system must eventually own consistent table, form-field, input, select/dropdown, autocomplete, button, loading, error, and empty-state behavior where reuse is demonstrated. Behavior, accessibility, and interaction remain component responsibilities; SCSS alone is not treated as an autocomplete or table abstraction.

Page-specific customization is supported through local component SCSS and inherited CSS custom-property overrides scoped beneath `:host` or an explicit page class. A feature may select an admitted variant such as compact density through a typed component input or host class. Page overrides must not mutate unrelated pages or depend on internal descendant structure that the shared component does not expose as part of its styling contract.

Tailwind utilities are migrated out of feature templates in stages. During migration, existing Tailwind behavior may remain only where not yet converted, but new or restructured UI uses the SCSS design system and shared UI primitives. Tailwind is removed from project configuration and dependencies after the final admitted template no longer requires it; the application must not retain two competing long-term sources of visual truth.

### Styling and encapsulation rules

- Do not use `::ng-deep`.
- Do not use `!important` except for a documented third-party integration that cannot be controlled through an admitted API, layer, or selector strategy.
- Do not hard-code a color, radius, shadow, spacing value, control height, or table density inside a feature when an admitted design token represents that concept.
- Keep local overrides scoped under `:host` or an explicit page/feature class.
- Style dropdown and autocomplete panels rendered outside a component host through the global overlay layer and theme tokens.
- Keep visible focus, keyboard operation, labels, error association, contrast, reduced-motion behavior, and responsive layout part of the shared component contract.
- Do not copy third-party component DOM selectors across pages. Wrap or configure a third-party control once behind the relevant shared UI boundary when one is admitted.
- A genuinely unique visualization or layout may keep feature-local SCSS, but it must consume global tokens for shared visual properties.
- Keep `_utilities.scss` small and semantic; do not recreate an unbounded utility framework after removing Tailwind.
- Update the Angular selector-prefix configuration and enforce `ll-` consistently before generating shared components.
- Global foundations must not contain selectors that reach into feature-private DOM. Use documented tokens, variants, host classes, and overlay contracts.

### Initial implementation sequence

1. Capture characterization tests for current Portfolio, Cash Ledger, Market Data, and Simulation workflows, including selected state, error/loading behavior, numeric formatting, chart lifecycle, responsive layout, and API calls.
2. Create the thin application shell and feature routes without changing styling or backend contracts.
3. Extract Simulation first because it currently owns the largest distinct workflow, including chart and Market Data coordination.
4. Implement the Item 4 Simulation pagination migration inside the extracted feature rather than changing the monolith and moving it later.
5. Extract Portfolios and Investment Accounts, then Cash Ledger, preserving route/query state and correction semantics.
6. Extract reusable Market Data access/models without turning Market Data into a global catch-all service.
7. After at least one extracted feature supplies real usage, introduce `styles.scss`, design tokens/themes, global foundations, and the first evidence-backed shared UI primitives.
8. Migrate remaining feature styles in bounded visual batches. Remove Tailwind only after the final usage scan, visual regression pass, dependency audit, and production build.
9. Add precise architecture/static checks for dependency direction, forbidden direct HTTP ownership, obsolete styling files, and disallowed styling escape hatches.

Each stage must remain independently buildable and reviewable. Do not combine the restructure with financial calculation changes, backend contract redesign, or unrelated visual redesign. Existing behavior, request semantics, API response/error handling, numeric presentation, accessibility, and responsive behavior are preserved unless a separately accepted item explicitly changes them.

### Implementation scope

- `web/ledgerlens-web/src/app/` application shell, routes, feature folders, shared UI, core API/configuration/layout, models, and tests
- `web/ledgerlens-web/src/styles.scss` and `web/ledgerlens-web/src/styles/` SCSS design-system sources
- Component `.css` files migrated to `.scss` where retained
- `web/ledgerlens-web/angular.json`, package scripts, and dependencies required by the SCSS/Tailwind migration
- Focused Angular architecture/static tests for the accepted ownership and styling rules
- Relevant architecture, frontend, testing, and project-state documentation as implementation stages become verified behavior

### Acceptance criteria

- `app.ts` is a thin application shell and does not own Portfolio, Investment Account, Cash Ledger, Simulation, or Market Data workflows.
- Business pages are reachable through standalone feature routes with a deterministic default route and fallback behavior; eager/lazy choices match the accepted bundle evidence.
- No feature page or presentational component constructs API URLs; typed feature data-access boundaries own HTTP communication.
- Feature state uses signals with explicit per-operation loading, failure, empty, and selected-item states and is scoped no wider than required; a store exists only where its lifecycle/shared-state responsibility is demonstrated.
- Refresh, direct URL entry, browser back/forward, fast page/sort/filter changes, and route destruction preserve the documented state and cannot be overwritten by stale HTTP responses.
- Repeated tables, selects/dropdowns, autocomplete controls, form fields, buttons, loading states, error states, and empty states use admitted shared UI primitives and global design tokens; unique/simple controls are not wrapped solely for uniformity.
- Changing an admitted global table or form-control token demonstrably updates every consuming feature without modifying those feature templates or local SCSS files.
- A page can override an admitted token or variant under `:host` or its page class without changing another page.
- Overlay content uses the same active tokens and theme as its invoking page and remains keyboard and screen-reader operable.
- No `::ng-deep` is introduced, and every exceptional `!important` use has a documented third-party justification and focused regression coverage.
- Feature SCSS does not duplicate admitted global colors, spacing, radii, control dimensions, table density, or focus styles with hard-coded values.
- Tailwind is not used by new or restructured feature UI and is fully removed once no admitted template depends on it.
- Existing API envelope/Problem Details behavior, financial calculations, numeric formatting, correction semantics, privacy boundaries, and loopback-only posture remain unchanged.
- Focused component and contract tests, Angular tests, production build, dependency/privacy checks, desktop and mobile visual QA, keyboard/focus checks, document-overflow checks, and `git diff --check` pass for the completed implementation stages.

### Decision record

- **Decision:** Accepted as Milestone 5A (feature structure) and Milestone 5B (SCSS/design system).
- **Owner rationale:** LedgerLens must scale by cohesive business feature while keeping one globally controlled visual language, but structural movement and visual-system migration require separate evidence so they do not obscure behavior regressions or create speculative abstractions.
- **Decision date:** 2026-09-24 (split into independently verified milestones; original direction accepted 2026-09-22).
- **Implementation evidence:** Not started.
