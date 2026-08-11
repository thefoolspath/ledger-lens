# Executive Summary

Last reviewed: 2026-08-11.

LedgerLens is planned as a localhost-only personal investment ledger and research application. It begins with US stocks and ETFs held through Dime, uses USD as the portfolio base currency, and adds a THB reporting view that exactly separates security-price and USD/THB effects.

The proposed stack is Angular 22.x with Tailwind CSS (not Bootstrap UI), ASP.NET Core/.NET 10 LTS, EF Core/Npgsql 10.x, PostgreSQL 18.x, NATS JetStream, and Aspire 13.4 stable. .NET, Aspire, Node, Angular CLI, Tailwind, and EF tooling are pinned per project; Docker Desktop remains the required machine-level container runtime. The backend uses coarse-grained Gateway, Portfolio Core, Market Data, Slip Import, Research, and Operations services with a database per service. Portfolio, ledger, lots, and financial calculations remain together in Portfolio Core to preserve atomic invariants.

The financial source of truth is an auditable ledger with explicit correction entries and trade lots. FIFO and average cost are both first-class reports derived from that ledger. Market and FX data are fetched when the user asks, always show provenance and freshness, and never expose provider keys to Angular.

Dime import uses private local storage, document hashing, embedded-text extraction before OCR, strict template parsing, field-level confidence, duplicate detection, and mandatory human confirmation. The specific OCR engine remains gated until representative Dime samples can be inspected privately and converted into newly created synthetic fixtures.

Version 1 intentionally excludes continuous operation, alerts, Windows notifications, authentication, network access, automatic trading, Thai securities, funds, gold, and local AI. Real data and secrets live outside the repository; all tracked examples are synthetic.

The next delivery is the manual portfolio vertical slice after platform, calculation-fixture, licensing, and repository-safety gates are approved.
