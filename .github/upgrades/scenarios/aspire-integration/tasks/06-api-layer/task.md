# 06-api-layer: Migrate PublicApi → Algowars.Api

Move source files from `src/PublicApi/` into `Algowars.Api/` and update all namespaces from `PublicApi.*` to `Algowars.Api.*`.

**Program.cs rewrite**: Replace current `Program.cs` with Aspire-aware version:
- Add `builder.AddServiceDefaults()` (OTEL, health checks, service discovery)
- Add `builder.AddInfrastructure()` (new Aspire-aware signature)
- Remove `builder.Services.RegisterAppSettings(...)` (Settings classes deleted)
- Remove `builder.Services.AddApplicationInsightsTelemetry(...)` (replaced by OTEL via ServiceDefaults)
- Keep: auth, CORS, controllers, rate limiting, MediatR, versioning, RBAC

**Controllers**: Rename `AccountController` → `UserController`. Update all route attributes and parameter types to use User terminology.

**appsettings.json cleanup**: Remove `ConnectionStrings` and `MessageBus` blocks (now Aspire-managed). Retain: `Logging`, `AllowedHosts`, `Auth0`, `Cors`, `ExecutionEngines`.

**MapDefaultEndpoints**: Add `app.MapDefaultEndpoints()` call (ServiceDefaults health check endpoints).

**Done when**: `Algowars.Api` builds; `Program.cs` uses `AddServiceDefaults()`; no `ConnectionStrings` or `MessageBus` config keys read in application code; `AccountController` is renamed to `UserController`.
