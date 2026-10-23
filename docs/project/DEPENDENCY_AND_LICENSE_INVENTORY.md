# Dependency and License Inventory

Last reviewed: 2026-09-23. Exact .NET versions are centralized in `Directory.Packages.props`; exact frontend versions and integrity hashes are in `web/ledgerlens-web/package-lock.json`.

The owner approved the free-use licenses for all direct dependencies admitted in the current application, development, and test baseline on 2026-08-26. This closes the Milestone 0 license gate. Candidate/evaluation-only dependencies remain unapproved until separately admitted under the policy below; provider API and data terms remain separate research gates.

| Candidate | Role | Version baseline | License/status | Decision |
| --- | --- | --- | --- | --- |
| .NET / ASP.NET Core / EF Core | API/application/data | SDK 10.0.302; EF 10.0.8 | MIT; Microsoft-supported | Accepted baseline |
| Aspire | orchestration/telemetry model | 13.4.6 | MIT upstream; owner-approved for current baseline | Accepted baseline; CLI project-local |
| Node.js | Angular toolchain runtime | 24 LTS, Angular-compatible patch | MIT and bundled third-party notices | Accepted baseline; portable/project-local planned |
| Angular | web UI | framework 22.1.1; CLI/build 22.1.3 | MIT | Accepted baseline |
| Tailwind CSS | web UI styling | 4.3.3 | MIT | Accepted baseline; project-local; Bootstrap UI excluded |
| Apache ECharts | simulation value and OHLCV visualization | 6.1.0 | Apache-2.0; production dependency audit rerun on 2026-09-02 with zero reported vulnerabilities | Accepted by owner direction; exact pin, direct lazy imports only, no Angular wrapper |
| PostgreSQL | database | 18.x | PostgreSQL License | Accepted baseline |
| Npgsql EF provider | PostgreSQL ORM provider and reverse engineering | 10.0.0 | PostgreSQL License | Accepted baseline; direct dependency of the service-owned Database/Migrations projects |
| YARP (`Yarp.ReverseProxy`) | Gateway reverse proxy | 2.3.0; service-discovery resolver 10.8.0 | MIT upstream; owner-approved for current baseline | Accepted foundation dependency |
| NATS server / NATS.Net / Aspire NATS integration | Durable asynchronous integration and health/telemetry | Aspire integration 13.4.6; server image selected by integration | Apache-2.0 upstream on research date; recheck exact image digest before release | Accepted foundation dependency |
| Mediator (`martinothamar/Mediator`) | Potential generated command/query dispatch | Not pinned | MIT upstream; full admission review still required | Candidate only; explicit handlers require no package in Version 1 baseline |
| `Google.Apis.Auth` | Candidate Google ID-token server verifier | 1.76.0 researched 2026-09-22 | Apache-2.0; Google-owned NuGet package; net6.0/netstandard compatible; approximately 536 KB; transitive/advisory/lockfile review pending | Candidate only for proposed plan 0006; no Gmail/OAuth API package or Angular wrapper admitted |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Candidate validation for closed-system Gateway-to-service tokens | Must align with the .NET 10 patch baseline | MIT/Microsoft platform package; exact version/transitives/advisories pending | Candidate only for proposed ADR-0020; first recheck whether required APIs are already in the shared framework |
| ASP.NET Core Identity MFA services | Candidate narrow TOTP/recovery/lockout implementation inside IdentityAccess | Must align with the .NET 10 shared-framework baseline | MIT/Microsoft-supported; exact storage/UI surface and package need determined by spike | Candidate only for plan 0006; compare against a custom TOTP aggregate, do not add local passwords or default UI implicitly |
| Tesseract | OCR candidate | 5.x | Apache-2.0 | Evaluation only |
| PaddleOCR | OCR candidate | 3.x | Apache-2.0 upstream; models/dependencies need separate review | Evaluation only |
| Gitleaks | secret scanning candidate | current supported | MIT upstream; recheck | Proposed |
| Ollama/vector extension | future AI | none | Not evaluated | Deferred |

## Admission policy

Before adding any runtime dependency, record purpose, owner, current maintenance, supported platforms, transitive/native components, package/model/data license, security posture, size/startup impact, alternatives, and removal strategy. Only dependencies with no fee for the intended local and future production use may be admitted. Trial, seat-based, runtime-metered, ambiguously dual-licensed, copyleft, or unclear package/model/data terms require explicit owner/legal review and are rejected by default. Prefer the built-in platform and permissive open-source licenses.

The Version 1 application baseline deliberately adds no mediator, object-mapping, validation, result, generic-repository, or unit-of-work framework. Aspire Dashboard is the observability UI; no additional logging/monitoring product is admitted. Repetition and a concrete use case must be demonstrated before another package is evaluated.

Provider APIs are services and licensed data, not ordinary library dependencies; evaluate account terms, display, retention, caching, redistribution, and attribution separately.

Twelve Data and Alpaca remain provider candidates rather than admitted dependencies. Their keys, account terms, feed entitlements, display/cache/retention rights, and raw data must not be inferred from the Apache ECharts admission or committed to the repository.
