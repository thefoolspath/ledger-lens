# Observability

Status: Accepted for the distributed foundation.

Aspire Dashboard is the only Version 1 technical observability UI. Every .NET executable applies `LedgerLens.ServiceDefaults` and exports OpenTelemetry signals to the Dashboard.

## Foundation signals

- **Health:** liveness plus readiness for the service's own PostgreSQL database and NATS connection.
- **Traces:** Angular/Gateway request correlation through YARP and backend HTTP; NATS spans when messages are introduced.
- **Metrics:** ASP.NET Core runtime/request metrics, outbound HTTP resilience, database/client health, and worker lifecycle.
- **Logs:** structured operational records containing service, environment, trace/correlation identifiers, event ID, and allowlisted diagnostic fields.

Do not add Seq, Grafana, Loki, Prometheus, or another backend until retention, production deployment, or querying requirements exceed Aspire Dashboard. Production telemetry can later remain backend-neutral through OTLP and an OpenTelemetry Collector.

Technical logs are not financial audit records. Exclude financial values, personal email, OCR text, account references, provider keys, connection strings, authorization data, and request bodies. Runtime telemetry and any retained logs remain outside the repository.

