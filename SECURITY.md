# Security Policy

## Current posture

LedgerLens is pre-implementation. Version 1 is designed for one person on one machine and must bind only to loopback. Local-only is a boundary, not a guarantee: malicious browser origins, local malware, weak OS accounts, logs, backups, and Git history remain threats.

## Never commit

- Real slips, OCR output, holdings, portfolio exports, database dumps, backups, or screenshots.
- API keys, passwords, connection strings, certificates, private keys, or data-protection keys.
- Logs or provider responses containing account, transaction, entitlement, or personal metadata.

## Future reporting

Before a public repository exists, enable private vulnerability reporting and publish a security contact. Do not disclose a vulnerability publicly until a fix or containment plan exists.

## Exposure response

If a secret or personal record is committed: stop distribution, revoke or rotate credentials, assess exposure, remove the material from history, coordinate any force-push, require collaborators to re-clone, and inspect forks, caches, releases, and CI artifacts. Adding an ignore rule does not remove history.

See [Security and Privacy](docs/architecture/SECURITY_AND_PRIVACY.md) and the [Repository Safety Plan](docs/project/REPOSITORY_SAFETY_PLAN.md).
