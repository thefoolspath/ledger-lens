# LedgerLens

## Foundation commands

Install the repository-scoped PowerShell command once, then open a new PowerShell window:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\install-powershell-cli.ps1
lg bootstrap
lg restore
lg build
lg test
lg run
```

The installer registers a small PowerShell profile function and does not modify
the user or machine `PATH`. The function accepts commands only while the current
directory is this repository or one of its subdirectories. Run `lg help`
to list every command. To remove it, run
`.\scripts\install-powershell-cli.ps1 -Uninstall`.

`run` starts the complete Aspire graph on loopback: Angular, Gateway, five APIs,
the Slip Import worker, PostgreSQL databases, NATS JetStream, and Aspire Dashboard.

LedgerLens is a proposed local web application for recording, reconciling, analysing, and researching a personal investment portfolio. The first usable release focuses on US stocks and ETFs held through Dime, a USD portfolio view, an additional THB reporting view, and deterministic separation of security-price and foreign-exchange effects.

Status: **distributed foundation implemented; runtime gate pending**. The source builds and the non-container test gates pass, but business logic remains intentionally absent. Container connectivity is still unverified because Docker Desktop could not start in the current Windows session; see [Project State](docs/project/PROJECT_STATE.md).

## Safety boundary

- Version 1 is loopback-only and has no authentication or authorization.
- Real slips, holdings, account identifiers, API keys, database files, exports, backups, OCR text, and logs must remain outside the repository.
- Examples, screenshots, parser fixtures, and calculation scenarios must be synthetic.
- LedgerLens records and explains data; it does not place trades or promise investment returns.

Start with [docs/README.md](docs/README.md). The source planning brief remains outside this folder at `../LedgerLens_Codex_Planning_Context.md` and is mapped in [requirements traceability](docs/project/REQUIREMENTS_TRACEABILITY.md).

## Confirmed product direction

- Angular frontend, ASP.NET Core backend, PostgreSQL, EF Core/Npgsql, and Aspire orchestration.
- Coarse-grained Aspire microservices with a database per service; Portfolio Core retains the atomic financial boundary.
- USD is the portfolio base currency; THB is a reporting currency.
- FIFO and average cost are both first-class report views derived from one auditable ledger.
- Market and FX data are fetched on demand; the system is not expected to run continuously.
- Alerts, Windows notifications, authentication, external access, automatic trading, Thai securities, funds, gold, and local AI are deferred.

## License status

MIT is the proposed license. No license file will be added until the owner confirms that decision.
