# Task 06 — Migrate PublicApi → Algowars.Api

## What Changed

### Program.cs (full rewrite)
- `builder.AddServiceDefaults()` — Aspire OTEL, health checks, service discovery
- `builder.AddInfrastructure()` — Aspire-aware DB + MQ connections
- `builder.Services.AddApplication()` — MediatR + FluentValidation + factories
- Removed `RegisterAppSettings`, `AddApplicationInsightsTelemetry`, manual MediatR license key
- Added `app.MapDefaultEndpoints()`, `app.UseRateLimiter()`
- No `ConnectionStrings` or `MessageBus` config keys in application code

### Controllers (MediatR-based, no app service dependencies)
- `UserController` — replaces `AccountController`; routes: `PUT /users`, `GET /users/find/profile/{username}`, `GET /users/me`, `GET /users/settings`
- `ProblemController` — `GET /problems/slug/{slug}`, `GET /problems`
- `SubmissionController` — `POST /submissions/execute`, `GET /submissions/{id}`
- `BaseApiController` — `Result<T>` → `IActionResult` helper

### Attributes
- `RequiresUserAttribute` (replaces `RequiresAccountAttribute`)
- `GlobalRateLimitAttribute`, `UserRateLimitAttribute` (ported unchanged)

### Authorization
- `RbacRequirement` + `RbacHandler` (ported, no App Insights dependency)

### Context
- `IUserContext` / `UserContext` — replaces `IAccountContext`/`AccountContext`

### Middleware
- `UserContextMiddleware` — resolves user by sub via MediatR `GetUserBySubQuery`; drops App Insights telemetry properties (OTEL via ServiceDefaults instead)
- `ExceptionMiddlewareExtensions` — global handler + `UseUserContext()` extension

### Extensions
- `AuthenticationExtensions` — Auth0 JWT Bearer
- `AuthorizationExtensions` — RBAC policies
- `RateLimitExtensions` — dynamic policy registration from controller attribute scan

### Configuration
- `appsettings.json` — keeps `Auth0`, `Cors`, `ExecutionEngines`, `Logging`; no `ConnectionStrings` or `MessageBus`
- `appsettings.Development.json` — debug log level

### Application layer fix
- All command/query records changed from `internal` to `public` (cross-assembly access from Api)

## Build Result
- Algowars.Api: 0 errors, 0 warnings
- Algowars.Application: 0 errors, 0 warnings
