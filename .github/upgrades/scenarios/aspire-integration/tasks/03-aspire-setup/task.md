# 03-aspire-setup: Create Algowars.ServiceDefaults and Algowars.AppHost

Create the two Aspire infrastructure projects.

**Algowars.ServiceDefaults**: Standard Aspire shared project (`IsAspireSharedProject=true`). Provides `AddServiceDefaults()` extension with OTEL (traces, metrics, logs), health checks, HTTP client resilience, and service discovery. Uses packages: `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`.

**Algowars.AppHost**: Aspire AppHost (`Aspire.AppHost.Sdk`). Orchestrates:
- PostgreSQL container resource named `algowars-db`
- RabbitMQ container resource named `algowars-mq`
- `Algowars.Api` project resource with references to both infrastructure resources

AppHost packages: `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.RabbitMQ`.

**Done when**: Both projects are in the solution; `Algowars.AppHost` builds; the `dotnet run --project Algowars.AppHost` launch path is correctly set up; `aspire --version` confirms CLI is available.
