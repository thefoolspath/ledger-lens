# Pre-implementation Research Gate

Last reviewed: 2026-08-11.

| Decision | Evidence state | Gate result |
| --- | --- | --- |
| .NET/Aspire baseline | Official support, CLI installation, and release evidence checked | Ready: project-local .NET 10 LTS + Aspire 13.4 stable baseline |
| Local toolchain | Machine inspected and isolation policy documented | Proposed; bootstrap and wrapper verification required |
| Angular baseline | Official release/support/compatibility checked | Ready: Angular 22.x baseline |
| PostgreSQL/Npgsql baseline | Official support and provider release notes checked | Ready: PostgreSQL 18.x + EF/Npgsql 10.x baseline |
| Coarse-grained microservices | Aspire communication, data ownership and financial consistency assessed | Ready; ADR-0015 accepted |
| Exact decimal/time policy | PostgreSQL documentation checked | Proposed; verify scale against samples |
| Dual cost views and FX attribution | Formula and authoritative context documented | Proposed; executable vectors required |
| Market-data abstraction | Provider capabilities compared | Ready |
| MVP provider selection | Pricing/capability checked | Blocked on cache/display/OSS terms confirmation |
| Bank of Thailand FX | Official API identified | Ready for prototype; key/availability test required |
| Dime extraction | Tool capabilities researched | Blocked on representative local-only samples |
| Localhost security | Controls identified | Proposed; browser security tests required |
| Backup/restore | Procedure defined | Proposed; clean restore test required |
| Local AI | No MVP need | Deferred |

No production implementation should start until the owner accepts the platform baseline and the calculation fixtures are reviewed. Market-data and Dime milestones have their own later gates and do not block manual portfolio core work.
