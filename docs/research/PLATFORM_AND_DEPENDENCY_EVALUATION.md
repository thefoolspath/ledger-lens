# Platform and Dependency Evaluation

Research date: 2026-08-11.

## Facts

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) lists .NET 10 as active LTS, released 2025-11-11, with support through 2028-11-14. The page listed patch 10.0.10 on the research date.
- [Aspire CLI installation](https://aspire.dev/get-started/install-cli/) identifies the stable channel and shows Aspire 13.4 as the validation baseline on the research date. The CLI can be installed as a .NET tool, which enables a project-local tool-path installation.
- [.NET install script reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script) supports an explicit `InstallDir` and `NoPath`; [.NET local tools](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use) documents repository-scoped tool restoration.
- [`AddJavaScriptApp`](https://learn.microsoft.com/en-us/dotnet/api/aspire.hosting.javascripthostingextensions.addjavascriptapp?view=dotnet-aspire-13.0) is the C# AppHost primitive for an npm-based JavaScript resource.
- [Angular releases](https://angular.dev/reference/releases) list Angular 22 as active, released 2026-06-03, with active support through 2027-06 and LTS through 2028-06. [Angular compatibility](https://angular.dev/reference/versions) lists Node 24.15+ as supported for Angular 22.0.x.
- [PostgreSQL versioning](https://www.postgresql.org/support/versioning/) lists PostgreSQL 18 as supported through 2030-11-14 and recommends the current minor release.
- [Npgsql EF provider 10 notes](https://www.npgsql.org/efcore/release-notes/10.0.html) document EF 10 support and PostgreSQL 18 features.

## Inference

The coherent baseline is .NET/EF/Npgsql major 10, Aspire 13.4 stable, Angular 22.x, Node 24 LTS-compatible range, and PostgreSQL 18.x. Project-local SDK and CLI paths reduce machine coupling and prevent the unsupported preview SDK currently installed from being treated as the project baseline.

## Recommendation

- Target supported GA releases only; resolve the current supported patch at implementation start and then pin the exact version rather than floating on `latest`.
- Install .NET into `.dotnet/`, Aspire into `.tools/aspire/`, and optional portable Node into `.tools/node/`; do not modify the permanent user/system `PATH`.
- Use project-local Angular CLI and `dotnet-ef` versions. Contributors and CI should call repository wrappers, not unqualified global tools.
- Verify Aspire package compatibility in a minimal spike before committing the repository foundation.
- Use Docker Desktop for local PostgreSQL because it is the required system-level runtime, while keeping container runtime details outside the domain.
- Prefer native HTTP clients and official framework integrations over provider SDK dependencies unless an SDK materially improves correctness and its license is approved.

## Unresolved

Update Docker Desktop from the inspected 4.27.1 installation to the offered 4.86.0 release and verify both client and server afterward. Validate the project-local GA .NET 10/Aspire bootstrap in a disposable spike before repository foundation implementation. Patch versions are deliberately not frozen in planning; the resolved versions will be pinned by the spike.

The point-in-time machine inventory and isolation contract are recorded in [Local Development Tooling](../project/LOCAL_DEVELOPMENT_TOOLING.md).
