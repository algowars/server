# Task 03 — Progress Details

## What Changed

### Algowars.ServiceDefaults — `Extensions.cs` rewritten
Full Aspire-standard service defaults implementation (replaced stub):

- `AddServiceDefaults<TBuilder>()` — orchestrates OTel, health checks, service discovery, HTTP resilience
- `ConfigureOpenTelemetry<TBuilder>()` — OTel logs (formatted messages + scopes), metrics (ASP.NET Core, HTTP client, runtime), traces (ASP.NET Core with health-check exclusion, HTTP client)
- `AddOpenTelemetryExporters<TBuilder>()` — OTLP exporter when `OTEL_EXPORTER_OTLP_ENDPOINT` is set
- `AddDefaultHealthChecks<TBuilder>()` — `/health` liveness self-check tagged `live`
- `MapDefaultEndpoints(WebApplication)` — maps `/health` and `/alive` in Development environments only

Namespace is `Microsoft.Extensions.Hosting` (Aspire convention — no assembly-qualified usage needed by consumers).

### Algowars.AppHost — `Program.cs` rewritten
Full Aspire orchestration (replaced stub):

```
postgres  = AddPostgres("algowars-postgres").WithDataVolume().AddDatabase("algowars-db")
rabbitmq  = AddRabbitMQ("algowars-mq").WithDataVolume().WithManagementPlugin()
api       = AddProject<Algowars_Api>("algowars-api")
			  .WithReference(postgres).WithReference(rabbitmq)
			  .WaitFor(postgres).WaitFor(rabbitmq)
```

Connection strings injected by Aspire:
- `ConnectionStrings:algowars-db` → consumed by `AddNpgsqlDbContext` in Infrastructure (task 05)
- `ConnectionStrings:algowars-mq` → consumed by MassTransit RabbitMQ in Infrastructure (task 05)

### New files
- `Algowars.AppHost/appsettings.json` — default log levels
- `Algowars.AppHost/appsettings.Development.json` — same (Aspire convention)

## Build Result
- `Algowars.ServiceDefaults` — ✅ 0 errors, 0 warnings
- `Algowars.AppHost` — ✅ 0 errors, 1 transitive NU1903 (MessagePack, Aspire SDK — not ours)

## Commit
`169db07` — "task 03: wire ServiceDefaults (OTel, health, resilience) and AppHost (postgres + rabbitmq resources)"
