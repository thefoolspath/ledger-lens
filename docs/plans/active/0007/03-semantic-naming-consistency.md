# 0007.03 — Establish system-wide semantic naming consistency

Last reviewed: 2026-09-28.

Parent: [0007 Code Review Decision Log](../0007-code-review-decision-log.md)

Group: Maintainability and dependency clarity

This file is a decision record and problem classification. It does not authorize source implementation beyond its explicit decision status and prerequisites.

## Related decisions

- [0007.04](04-collection-correctness-pagination-and-sorting.md) — `GetList` to `GetPage` changes are contract migrations, not cosmetic renames.
- [0007.05](05-angular-feature-architecture-and-scss-system.md) — feature folders, shared models, and frontend identifiers need the same domain vocabulary.
- [0007.07](07-persistence-port-decomposition.md) — store and port names must follow capability ownership after boundaries stabilize.


**Status:** Pending owner decision and system-wide assessment.

### Current evidence

`SimulationAccountsCreateHandler.ToListItem(SimulationAccount account)` maps one domain entity to a `SimulationAccountListItem`. The word `ListItem` describes the DTO's original presentation role rather than the cardinality of the method result, so the method does not create a list. The same mapper and DTO are also reused by `SimulationAccountsGetOneHandler` to build a single-account overview. This makes both the type name and method name narrower than their actual responsibilities, while placing a shared mapping function under the create operation's handler.

The current naming standard also states that C# methods, parameters, and local variables use `camelCase`. That rule conflicts with standard .NET casing, where method names use `PascalCase` while parameters and local variables use `camelCase`. This must be resolved as part of the wider assessment rather than copied into new code.

### Review concern

Renaming only `ToListItem` would correct one symptom while leaving the same ambiguity available elsewhere. Classes, records, interfaces, methods/functions, parameters, local variables, folders, files, API operation keys, Angular identifiers, and shared mapping helpers need one coherent naming direction across the system.

A name must describe the identifier's actual business meaning, responsibility, result shape, and cardinality. It must not imply list, page, single-item selection, creation, persistence, or another behavior that the identifier does not perform. Reused types and functions must be named for their shared meaning and owned by a shared cohesive location rather than by one incidental caller.

This assessment is semantic, not merely mechanical casing cleanup. It must review related identifiers together so a rename does not create new inconsistencies across API, Application, Domain, Infrastructure, Angular, tests, and documentation.

### Assessment scope

- Inventory production C# and TypeScript identifiers, prioritizing public types, shared DTOs, handler and store methods, mappers, API contracts, Angular API methods, and names reused by multiple operations.
- Compare class/type nouns, method/function verbs, result types, cardinality, side effects, and ownership against actual behavior and call sites.
- Identify misleading role suffixes such as `ListItem`, `Manager`, `Helper`, `Data`, or `Info` when the implementation has a broader, narrower, or different responsibility.
- Verify that operation-specific identifiers follow the accepted `OperationKey`, while genuinely shared concepts use stable domain vocabulary rather than an arbitrary operation name.
- Correct the C# casing rule so types and methods use `PascalCase`, parameters and local variables use `camelCase`, and standard .NET acronym casing remains consistent.
- Review filenames, namespaces, folders, tests, architecture rules, and documentation together with each accepted rename.
- Exclude generated code, EF migrations, snapshots, external protocol fields, and persisted schema identifiers unless a separately reviewed compatibility or migration plan explicitly includes them.
- Preserve public API JSON fields, routes, database schema, and other external contracts unless the owner explicitly approves a contract migration.
- Record each candidate with the current symbol, actual behavior, direct callers, owning layer, contract/persistence impact, proposed name, priority, and evidence. An inventory entry is not rename approval.
- Do not enforce semantic quality through brittle keyword bans. Names such as `Data`, `Info`, `Helper`, or `Manager` are findings only when their concrete responsibility is misleading.
- Treat the incorrect C# method-casing sentence in the governing naming standard as a confirmed documentation defect and correct it independently from source rename batches.

### Options

#### Option A — fix only the reported simulation mapper

- Rename `SimulationAccountListItem` and `ToListItem` locally and move the mapper to a shared owner.
- Smallest immediate diff, but it does not prevent equivalent ambiguous naming elsewhere.

#### Option B — perform a governed system-wide naming assessment and staged correction

- First produce an evidence-backed inventory of ambiguous or inconsistent identifiers across the admitted production code.
- Define the intended domain vocabulary, semantic naming rules, casing rules, ownership rules, and compatibility boundaries before source renames.
- Group accepted renames into cohesive, independently verifiable stages rather than one unreviewable repository-wide rename.
- Keep each implementation batch within one cohesive feature or shared concept and do not combine it with behavior, schema, pagination, or styling changes unless the rename is required by that accepted contract migration.
- Add focused architecture/static checks where a rule can be enforced reliably without rejecting legitimate domain names.
- Use `SimulationAccountListItem` / `ToListItem` as the first recorded example, not as the complete scope.

### Initial review position

The owner requires the assessment to cover naming consistency throughout the system and explicitly rejects a local-only correction. Option B is therefore the current candidate. Inclusion in this plan records that direction but does not authorize implementation until the inventory, compatibility impact, and staged rename set have been reviewed.

### Implementation scope if Option B is accepted

- Current production C# and Angular/TypeScript source, excluding the explicit compatibility exclusions above
- Direct call sites, request/response types, mappers, tests, filenames, namespaces, and feature folders affected by each accepted rename
- `docs/architecture/API_AND_FEATURE_NAMING_STANDARD.md`
- Architecture/static naming tests where rules can be expressed without brittle keyword bans
- Relevant active plans and project documentation that reference renamed identifiers
- `docs/project/PROJECT_STATE.md` only after verified behavior or project state materially changes

### Acceptance criteria if Option B is accepted

- The assessment covers the admitted production system rather than only `SimulationAccountListItem` and `ToListItem`.
- Every accepted class/type and method/function name matches its actual domain meaning, responsibility, result shape, cardinality, side effects, and ownership.
- Related identifiers use one consistent vocabulary across backend, frontend, tests, and documentation.
- A shared type or mapper is not named for one incidental caller or stored under an operation that does not own it.
- C# casing guidance matches .NET conventions and no governing document gives contradictory rules.
- No rename silently changes an API route, JSON contract, persistence schema, financial behavior, authorization boundary, transaction behavior, ordering, or null semantics.
- Generated and externally constrained identifiers remain unchanged unless separately approved.
- Relevant builds, tests, architecture checks, repository naming scans, and `git diff --check` pass for every implementation stage.

### Decision record

- **Decision:** Pending system-wide assessment and owner approval of the staged rename set.
- **Owner rationale:** Naming must be coherent and unambiguous across classes and functions throughout the system; correcting only the reported case is insufficient.
- **Decision date:** Pending.
- **Implementation evidence:** Not started; `SimulationAccountListItem` / `ToListItem` is the first recorded assessment case.
