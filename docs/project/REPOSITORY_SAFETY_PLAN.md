# Repository Safety Plan

Status: Accepted design; controls are not yet implemented.

## Ignore design

Future `.gitignore` must cover user secrets, environment files except documented examples, application-data overrides, database data/dumps, slips, OCR artifacts, imports/exports, backups, logs, coverage/test artifacts, local certificates/keys, `.dotnet/`, `.tools/`, Aspire state, NuGet/npm caches, downloaded installers, IDE files, Angular/.NET build outputs, and OS metadata. Future `.dockerignore` must exclude the same private/runtime areas plus Git history and local build/test output.

Local SDKs, CLIs, portable runtimes, installers, and package caches are reproducible machine artifacts and must not be committed. Git tracks only pinned version declarations, bootstrap/wrapper definitions, approved checksums where applicable, and documentation. Tool folders must never be repurposed for secrets or runtime investment data.

Ignore patterns are not the primary boundary: runtime paths default outside the repository and startup must refuse a private-data root inside the working tree unless an explicit safe test mode uses synthetic data.

## Automated controls

- Pre-commit and CI secret scanning, with Gitleaks as the initial candidate after license review.
- CI forbidden-path/extensions and high-risk filename checks.
- GitHub secret scanning and push protection where available.
- CodeQL for C# and JavaScript/TypeScript, Dependabot/dependency review, branch protection, required checks, and private vulnerability reporting.
- Documentation/test scanner for real-looking account references and prohibited fixture directories.

## Safe samples

Generate synthetic slips and portfolios from invented data. Do not blur, crop, recolour, or redact a real slip for publication. Provider response fixtures must be hand-authored against documented schemas and carry no copied licensed dataset or account metadata.

## Incident response

Stop distribution; rotate/revoke; assess logs, clones, forks, caches, releases, and artifacts; clean history with an appropriate tool; coordinate force-push and re-clone; document lessons and strengthen controls.

References: [GitHub secret scanning](https://docs.github.com/en/code-security/concepts/secret-security/about-alerts) and [security policy](../../SECURITY.md).
