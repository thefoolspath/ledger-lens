# Foundation Benchmark Results

Last reviewed: 2026-08-12. Status: Milestone 1 evidence; repeat samples remain required for percentile claims.

## Environment

- Windows 11 Home Single Language 10.0.26200.
- 12th Gen Intel Core i5-12500H, 16 logical processors, 15.6 GiB installed RAM.
- Project-local .NET SDK 10.0.302, Aspire 13.4.6, Node 24.19.0, and npm 11.17.0.
- Docker Desktop 4.86.0 with client/server 29.7.2; PostgreSQL 18.4 and NATS 2.14 containers.
- Debug AppHost execution with already restored dependencies and locally available container images. PostgreSQL and NATS named volumes remained outside the repository. No financial, personal, or provider data was used.

## Startup observations

| Observation | Start definition | Ready definition | Result |
| --- | --- | --- | --- |
| First measured process start | Current `LedgerLens.AppHost.exe` creation at 16:05:58.142 +07:00 after prior containers were stopped | The first recorded probe through Angular returned HTTP 200 from all five Gateway routes at 16:06:24.885 +07:00 | At most 26.743 seconds; an upper bound because this run was not continuously polled |
| Warm process restart | Wrapper launch at 16:11:45.267 +07:00 with dependencies and images already available | Polling through Angular returned HTTP 200 from all five Gateway routes at 16:12:17.445 +07:00 | 32.178 seconds across 14 polling attempts |

The warm observation exceeds the proposed 30-second target by 2.178 seconds. One observation cannot establish p95 and the performance budget is still a proposed investigation target, so this does not invalidate Milestone 1 correctness. Collect repeat runs before changing the target or optimizing startup.

## Memory observation

After the first measured graph was healthy and the Angular request had completed, the attributed AppHost/DCP process trees used approximately 886.3 MiB working set. PostgreSQL used 78.88 MiB and NATS used 8.359 MiB according to one `docker stats --no-stream` sample, for an approximate observed total of 973.5 MiB. This is a point-in-time working-set observation, not peak private bytes or a percentile.

## Runtime and observability evidence

- Aspire Dashboard showed 16 resources: all long-running resources were `Running`, while `web-installer` was finished as designed.
- The Angular shell showed Portfolio Core, Market Data, Slip Import, Research, and Operations as connected.
- Dashboard traces showed each Gateway route with two Gateway spans and one owning-backend span. Example trace `85a65deebbd00273616f8a782c2a1773` covered `GET /api/portfolio/{**catch-all}` with Gateway and Portfolio API spans.
- Structured logs displayed resource, message, and trace columns; the same routed requests included proxy and HTTP 200 records with trace identifiers.
- Gateway metrics exposed `http.server.request.duration` with GET/200 data for all five routed paths and `/health`.
- An explicitly enabled clean distributed-app test run passed two of two tests in 33 seconds. A concurrent run correctly failed to start `web` while an interactive AppHost already owned the Angular development port; the clean rerun is the acceptance evidence.

## Retest command

Run `lg test` with no interactive `lg run` active. The wrapper enables the guarded distributed test, which uses ephemeral credentials and disables named volumes.
