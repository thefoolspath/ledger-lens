# Localhost Security Evaluation

Research date: 2026-08-11.

## Threats

- A malicious website attempts cross-origin writes to an unauthenticated loopback API.
- Host-header/DNS-rebinding behaviour reaches a service that trusts `localhost` semantically.
- A development dashboard, database, or OCR endpoint binds beyond loopback.
- A crafted PDF/image exhausts CPU, memory, disk, or parser libraries.
- Secrets or personal values leak through logs, errors, backups, source control, or Angular bundles.
- Local malware or another OS account reads runtime storage.

## Official guidance used

- [ASP.NET Core CORS guidance](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0) explains that CORS selectively relaxes browser same-origin restrictions; it is not authentication.
- [GitHub secret-scanning concepts](https://docs.github.com/en/code-security/concepts/secret-security/about-alerts) describe repository scanning and push-protection alerts.

## Recommendations

- Same-origin frontend/API, loopback sockets, strict host allowlist, exact origin checks, no wildcard CORS, and no external development tunnels.
- Require JSON and a custom header for mutations; validate Origin/Referer and reject simple cross-origin form content types.
- Apply size/page/time/memory limits and process OCR with least privilege and bounded concurrency.
- Keep secrets and runtime data outside the repo; enable pre-commit, CI, GitHub secret scanning/push protection, CodeQL, dependency updates, and private vulnerability reporting when GitHub is initialized.
- Treat full-disk encryption, OS account security, and filesystem ACLs as deployment prerequisites for sensitive personal data.

## Validation gate

Automated integration/browser tests must prove non-loopback binding fails, unapproved Host/Origin requests fail, state-changing simple requests fail, oversized/malformed documents fail safely, and error responses contain no sensitive content.
