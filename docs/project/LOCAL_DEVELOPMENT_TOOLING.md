# Local Development Tooling

Last reviewed: 2026-09-02. Status: Implemented and runtime-verified.

This document records the inspected Windows environment, the supported target toolchain, and the boundary between project-local and system-level dependencies. The project-local bootstrap and development wrappers are implemented, and Docker runtime verification passed on 2026-08-12.

## Machine audit

| Tool | Inspected state | Readiness |
| --- | --- | --- |
| Git | 2.38.1 | Installed; update recommended before publishing the repository |
| VS Code and Chrome | Installed | Ready for documentation and research |
| Python | 3.13.5 | Installed; not selected as the future OCR runtime |
| Node.js | 24.4.1 | Installed globally; below the Angular 22 Node 24 minimum of 24.15 |
| npm | 11.4.2 | Installed globally |
| .NET SDK | 8.0.129 and 10.0.400 preview | Stable .NET 10 SDK is missing; preview must not be used as the project baseline |
| Aspire CLI | Not found | Required for the implementation foundation |
| Angular CLI | Not found globally | Expected as a project-local npm development dependency |
| `dotnet-ef` | Global 9.0.7 | Must not be used for the EF Core 10 project; use a pinned local tool |
| Docker Desktop | 4.86.0; client/server 29.7.2 | Verified healthy on 2026-08-12 |
| Docker daemon | Desktop Linux context; Engine 29.7.2 | Verified by the repository doctor and distributed runtime tests |
| PostgreSQL CLI | `psql` and `pg_dump` not found | Not required on Windows when PostgreSQL is container-managed |
| Gitleaks | Not found | Required before the first public commit/push |
| Tesseract and PaddleOCR | Not found | Deferred to the Dime extraction evidence milestone |
| GitHub CLI | Not found | Optional until repository publication/automation |
| Ollama | Desktop application installed; CLI not on `PATH` | Deferred with local AI |

The audit is a point-in-time observation, not a reproducible environment declaration. Bootstrap verification becomes the source of truth once implemented.

## Supported baseline

| Component | Baseline | Policy |
| --- | --- | --- |
| .NET / ASP.NET Core / EF Core | .NET 10 LTS / EF Core 10 | Resolve the current supported GA SDK once, then pin the exact SDK |
| Aspire | 13.4 stable | Pin the exact CLI and package versions validated by the platform spike |
| Node.js | Node 24 LTS | Use a release compatible with Angular 22, currently 24.15 or later |
| Angular | 22.x | Install CLI and framework packages locally through the workspace package manager |
| Tailwind CSS | Stable Angular-compatible release | Install and pin locally with the future Angular workspace; do not add Bootstrap UI packages |
| PostgreSQL | 18.x | Use the current supported minor through an Aspire-managed container |
| Npgsql | 10.x | Keep the EF provider major aligned with EF Core 10 |

The policy means the newest supported stable/LTS-compatible release at the decision date, not a floating `latest` dependency on every restore. Version updates are explicit reviewed changes.

## Project-local layout

```text
LedgerLens/
|-- .dotnet/                 # local stable .NET 10 SDK; ignored
|-- .tools/
|   |-- dotnet-ef/           # pinned EF Core 10 CLI; ignored
|   `-- node/                # optional portable Node 24 LTS; ignored
|-- .a/                      # local Aspire CLI; short path avoids Windows MAX_PATH; ignored
|-- global.json              # exact SDK version and local-only SDK search path
|-- scripts/                 # bootstrap, development wrapper, and PowerShell command installer
|-- lg.cmd                   # friendly development CLI entry point
`-- web/ledgerlens-web/      # Angular workspace and pinned JavaScript tooling
```

The bootstrap and wrappers:

1. Install the exact GA .NET 10 SDK into `.dotnet/` with the official install script using `InstallDir` and `NoPath`.
2. Configure `global.json` to search `.dotnet` without falling back to the machine preview SDK.
3. Install the pinned Aspire CLI with the local SDK and `--tool-path .a`; use a temporary short drive alias only during restore because this repository's OneDrive path makes the NuGet tool layout exceed Windows `MAX_PATH` under `.tools/aspire`.
4. Restore the pinned Node/Angular and EF Core CLI toolchains without global installation. The EF tool version is declared in `toolchain.json` and installed under `.tools/dotnet-ef`.
5. Be idempotent, avoid permanent user/system `PATH` changes, and fail with a clear restore instruction when a required local tool is absent.
6. Provide wrapper commands that contributors and CI use instead of unqualified global `dotnet`, `aspire`, `node`, or `ng` commands.

`lg.cmd` forwards commands to `scripts/dev.ps1`, which resolves only the pinned tools inside the repository. `scripts/install-powershell-cli.ps1` registers an `lg` function in the current user's PowerShell profile without modifying the user or machine `PATH`. The function refuses to run unless the current directory is the LedgerLens repository or one of its subdirectories. The installer is idempotent, removes the former `thefools` profile block during migration, and supports `-Uninstall`.

Central package versions must remain compatible with every pinned consumer. Aspire 13.4.6 requires `Microsoft.Extensions.Hosting` 10.0.8 or later and `Microsoft.AspNetCore.Mvc.Testing` 10.0.10 raises the solution-wide minimum to 10.0.10, so the repository centrally pins Hosting 10.0.10 rather than the .NET 10.0.0 baseline. This prevents `NU1109` during solution restore and `lg run`.

The wrappers set `NUGET_PACKAGES` to the ignored short path `.n`. This keeps restored assembly paths below the legacy Windows 260-character boundary even when the repository itself is under the long OneDrive workspace path; bootstrap, development, and database commands all use the same cache.

`lg db` routes to the guarded Portfolio Core hybrid database workflow. It supports `scaffold`, `migration`, `check`, and reviewable `script` generation. It never applies a migration or writes a migration-history row. See [Hybrid Database Workflow](../architecture/HYBRID_DATABASE_WORKFLOW.md).

## System-level exceptions

- Docker Desktop remains a machine-level OCI runtime. After updating, `docker version` must report both client and server and `docker compose version` must succeed.
- Git and an editor/browser remain machine-level contributor tools.
- PostgreSQL runs as a container resource; PostgreSQL server and CLI packages are not required on Windows. Backup and restore commands may run within a version-matched container.
- OCR must use a separately evaluated container or isolated runtime so PaddleOCR/Tesseract dependencies do not alter the base project toolchain.
- Gitleaks must be pinned as a project/CI control before public repository work. GitHub CLI remains optional.

## Repository boundary

`.dotnet/`, `.tools/`, `.a/`, downloaded installers, Aspire state, package-manager caches, and generated tool outputs must be ignored. Git tracks only version declarations, bootstrap/wrapper definitions, checksums where applicable, and documentation. Tool directories must never contain secrets or runtime investment data.

The root `.gitignore` also excludes .NET test-result directories and TRX files, local appsettings overrides, local certificate/private-key files, and user-specific IDE or Windows metadata. `.env.example` remains explicitly trackable so contributors can document required environment-variable names without committing values.

## Fixed local user setup

Milestone 2 requires a stable UUIDv7 and owner-supplied email in the AppHost User Secrets store alongside the PostgreSQL password. Do not put either value in tracked settings or shell scripts:

```powershell
.\lg.cmd dotnet user-secrets set "Parameters:fixed-user-id" "<stable-uuidv7>" --project "src/LedgerLens.AppHost/LedgerLens.AppHost.csproj"
.\lg.cmd dotnet user-secrets set "Parameters:fixed-user-email" "<owner-email>" --project "src/LedgerLens.AppHost/LedgerLens.AppHost.csproj"
```

Portfolio Core validates the identifier version and email shape during startup. The profile is bootstrapped idempotently on the first portfolio creation and no personal bootstrap value is embedded in a migration.

## Verification contract

The implementation foundation is ready only when:

- wrapper-reported .NET and Aspire versions match the pinned GA versions;
- the local SDK is used even while a global preview SDK is installed;
- no bootstrap step permanently changes user or system `PATH`;
- a clean checkout can restore the same tool versions;
- Docker reports a running client/server pair after update;
- PostgreSQL starts through Aspire without a Windows PostgreSQL installation; and
- missing local tools produce actionable restore errors.

## Official evidence

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy), accessed 2026-08-11.
- [.NET install script reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script), accessed 2026-08-11.
- [.NET local tools](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use), accessed 2026-08-11.
- [Aspire CLI installation](https://aspire.dev/get-started/install-cli/), accessed 2026-08-11; the stable validation example is Aspire 13.4.
- [Angular version compatibility](https://angular.dev/reference/versions), accessed 2026-08-11.
- [Node.js releases](https://nodejs.org/en/about/previous-releases), accessed 2026-08-11.
- [PostgreSQL versioning policy](https://www.postgresql.org/support/versioning/), accessed 2026-08-11.
- [Docker Desktop release notes](https://docs.docker.com/desktop/release-notes/), accessed 2026-08-11.
