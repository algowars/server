# Aspire Integration + Modernization Plan

## Overview

**Target**: Algowars Server — 4 existing projects (ApplicationCore, Infrastructure, PublicApi, UnitTests) under `src/` and `tests/`
**Scope**: Large restructuring — ~50 source files across 4 projects, new DDD domain layer, Aspire orchestration, EF migrations

---

## Tasks

### 01-project-scaffolding: Set up new project structure at root

Create the target directory layout at repo root. All seven projects get new `Algowars.*`-prefixed folders and `.csproj` files. The solution file (`Algowars.slnx`) is rewritten to reference the new paths without `src/` or `tests/` prefixes. Introduce `Directory.Packages.props` (central package management) and `nuget.config` from the reference branch pattern. The old `src/` and `tests/` folders are removed from the solution but their files remain on disk until subsequent tasks move them.

Affected projects (new paths → old source):
- `Algowars.Api/` ← `src/PublicApi/`
- `Algowars.Application/` ← `src/ApplicationCore/`
- `Algowars.Domain/` ← *(new)*
- `Algowars.Infrastructure/` ← `src/Infrastructure/`
- `Algowars.AppHost/` ← *(new)*
- `Algowars.ServiceDefaults/` ← *(new)*
- `Algowars.UnitTests/` ← `tests/UnitTests/`

**Done when**: Solution loads with all 7 projects visible at root level; `dotnet restore` succeeds on all projects; old `src/` and `tests/` project folders are removed from `Algowars.slnx`.

---

### 02-domain-layer: Create Algowars.Domain

Build out the pure DDD domain layer. This project has no external NuGet dependencies — only C# and the .NET BCL.

**SeedWork** (from reference branch): `Entity`, `AggregateRoot`, `DomainException`, `IAggregateFactory`.

**Users aggregate** (merges reference branch `User` with current `AccountModel`):
- Entity: `User` — properties: `Id`, `Sub`, `Username`, `Bio`, `ImageUrl`, `UsernameLastChangedAt`; domain methods: `ChangeUsername`, `UpdateBio`, `UpdateImageUrl`; enforces 30-day username cooldown
- Value objects: `Username`, `Bio`, `ImageUrl`
- Exceptions: `InvalidUsernameException`, `InvalidUserSubException`, `InvalidBioException`, `InvalidImageUrlException`, `UsernameCooldownException`
- Repository interface: `IUserRepository`

**Problems aggregate** (from reference branch):
- Entities: `Problem`, `ProblemVersion`, `CodeTemplate`, `Example`, `TestCase`
- Enums: `DifficultyTier`, `ProblemStatus`
- Value objects: `Title`, `Question`, `Slug`, `Difficulty`, `MemoryLimit`, `TimeLimit`
- Exceptions: `InvalidTitleException`, `InvalidQuestionException`, `InvalidSlugException`, `InvalidDifficultyException`, `InvalidMemoryLimitException`, `InvalidTimeLimitException`, `ProblemVersionImmutableException`, `ProblemVersionNotFoundException`
- Repository interface: `IProblemRepository`

**Submissions aggregate** (from reference branch):
- Entities: `Submission`, `SubmissionResult`
- Enums: `SubmissionStatus`, `SubmissionType`, `SubmissionResultStatus`
- Value objects: `SourceCode`
- Exceptions: `InvalidSourceCodeException`, `InvalidSubmissionStateException`, `SubmissionResultNotFoundException`, `SubmissionNotCompleteException`
- Repository interface: `ISubmissionRepository`

**Languages aggregate** (from reference branch):
- Value objects: `LanguageName`, `LanguageSlug`, `LanguageVersion`

**Done when**: `Algowars.Domain` builds without errors or warnings; all entities, value objects, exceptions, and repository interfaces are present; no external NuGet package references exist in the project file.

---

### 03-aspire-setup: Create Algowars.ServiceDefaults and Algowars.AppHost

Create the two Aspire infrastructure projects.

**Algowars.ServiceDefaults**: Standard Aspire shared project (`IsAspireSharedProject=true`). Provides `AddServiceDefaults()` extension with OTEL (traces, metrics, logs), health checks, HTTP client resilience, and service discovery. Uses packages: `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`.

**Algowars.AppHost**: Aspire AppHost (`Aspire.AppHost.Sdk`). Orchestrates:
- PostgreSQL container resource named `algowars-db`
- RabbitMQ container resource named `algowars-mq`
- `Algowars.Api` project resource with references to both infrastructure resources

AppHost packages: `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.RabbitMQ`.

**Done when**: Both projects are in the solution; `Algowars.AppHost` builds; the `dotnet run --project Algowars.AppHost` launch path is correctly set up; `aspire --version` confirms CLI is available.

---

### 04-application-layer: Migrate ApplicationCore → Algowars.Application

Move all source files from `src/ApplicationCore/` into `Algowars.Application/` and update every namespace from `ApplicationCore.*` to `Algowars.Application.*`.

**Account → User rename** (all files in `Queries/Accounts/`, `Services/AccountAppService`, `Interfaces/Repositories/IAccountRepository`, `Dtos/Accounts/`, `Domain/Accounts/`):
- `IAccountRepository` → `IUserRepository` (moved to Domain in task 02; remove local copy)
- `AccountModel` → removed (domain entity `User` from `Algowars.Domain` replaces it; Application layer works with domain types or lightweight DTOs)
- `GetAccountBySub*` → `GetUserBySub*`
- `GetProfileAggregate*` → keep name, update Account references to User
- `AccountAppService` → `UserAppService`
- `AccountDto`, `ProfileAggregateDto`, `ProfileSettingsDto` → keep or rename as appropriate

**Settings removal**: Delete `Settings/ConnectionStringsSettings.cs`, `Settings/CorsSettings.cs`, `Settings/MediatRSettings.cs`, `Settings/ISettings.cs` — these are replaced by Aspire-managed connections and direct `IConfiguration` reads.

**DDD command pattern**: Bring in `ICommand`, `ICommandHandler<TCommand>`, `AbstractCommandHandler` from reference branch.

**Project reference**: `Algowars.Domain` (replaces domain models that lived in ApplicationCore).

**Done when**: `Algowars.Application` builds without errors or warnings; no `ApplicationCore.*` namespaces remain; Settings classes removed; Account-named types renamed to User.

---

### 05-persistence-layer: Migrate Infrastructure → Algowars.Infrastructure

Move source files from `src/Infrastructure/` into `Algowars.Infrastructure/` and update all namespaces from `Infrastructure.*` to `Algowars.Infrastructure.*`.

**DbContext rename**: `AppDbContext` → `AlgoWarsDbContext`. Keep all existing entity configurations (Problems, ProblemSetups, TestSuites, Submissions, Languages). Add `Users` DbSet using a `UserDataModel` persistence model (replaces `AccountEntity`).

**Aspire DB connection**: Replace `services.AddDbContext<AppDbContext>(o => o.UseNpgsql(config["ConnectionStrings:DefaultConnection"]))` with `builder.AddNpgsqlDbContext<AlgoWarsDbContext>("algowars-db")`. Change registration method signature to accept `IHostApplicationBuilder`.

**Aspire MQ connection**: Replace manual `MessageBus:RabbitMQ:*` / `MessageBus:AzureServiceBus:*` appsettings reads with `builder.Configuration["ConnectionStrings:algowars-mq"]` (Aspire injects this automatically from AppHost).

**Repository**: Implement `IUserRepository` (replaces `IAccountRepository`). Retain `ProblemRepository` and `SubmissionRepository` with namespace updates.

**Mappings**: Update `AccountMappings` → `UserMappings` (AccountEntity → UserDataModel, AccountModel → User domain entity).

**Package update**: Replace `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL` explicit package references with `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` (bundles both + Aspire integration).

**Done when**: `Algowars.Infrastructure` builds; `IUserRepository` is implemented; `AlgoWarsDbContext` uses Aspire connection; no manual `ConnectionStrings:DefaultConnection` or `MessageBus:*` reads remain.

---

### 06-api-layer: Migrate PublicApi → Algowars.Api

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

---

### 07-test-project: Migrate UnitTests → Algowars.UnitTests

Move test files from `tests/UnitTests/` into `Algowars.UnitTests/` and update all namespaces from the old `ApplicationCore.*` / `Infrastructure.*` / `PublicApi.*` patterns to the new `Algowars.*` equivalents.

Rename account-related test files and classes:
- `AccountModelTests.cs` → `UserTests.cs` (updated to test the new DDD `User` entity)
- `GetAccountBySubHandlerTests.cs` → `GetUserBySubHandlerTests.cs`
- `GetProfileAggregateHandlerTests.cs` → update Account references
- `GetProfileSettingsHandlerTests.cs` → update Account references

Update project references to `Algowars.Application`, `Algowars.Infrastructure`, `Algowars.Api`.

**Done when**: `Algowars.UnitTests` builds; all tests pass; no old namespace references remain.

---

### 08-ef-migrations: Add EF Core code-first migration

Add a design-time `IDesignTimeDbContextFactory<AlgoWarsDbContext>` to `Algowars.Infrastructure` so EF tooling can create migrations without a running app.

Run `dotnet ef migrations add InitialCreate --project Algowars.Infrastructure --startup-project Algowars.Api` to generate the initial migration covering the full current schema (Users, Problems, ProblemSetups, Submissions, TestSuites, Languages, etc.).

Remove any residual references to old manual SQL migration scripts.

Validate the full solution build is clean (all 7 projects, zero errors, zero warnings). Run the test suite (`dotnet test Algowars.UnitTests`) and confirm all tests pass.

**Done when**: A valid `Migrations/` folder exists in `Algowars.Infrastructure`; the migration applies cleanly against an empty PostgreSQL database; full solution builds with zero errors and zero warnings; all unit tests pass.

---

### 09-submission-outbox-redesign: Per-step outbox ledger with retry tracking

Redesign the submission processing outbox from a single mutating row (where `SubmissionOutboxType` is overwritten as the submission advances) into an append-only ledger where **each pipeline step inserts its own row**. This gives a per-step retry count and an independent status for every stage, making it easy to answer "the Execute step needed 3 attempts; Evaluate needed 1".

#### Domain design (`Algowars.Domain/Submissions/Outbox/`)

**`SubmissionOutboxStep`** enum — the five pipeline stages, each maps to one row class:
```
Execute = 1         // Initial code execution request to Judge0
PollExecution = 2   // Polling Judge0 for execution result tokens
Evaluate = 3        // Comparing execution output against expected answers
EvaluationPoll = 4  // Polling final evaluation state before completing
```

**`SubmissionOutboxStatus`** enum — per-row lifecycle:
```
Pending    = 1  // Row created, not yet picked up
Processing = 2  // Worker is actively working on this step
Retrying   = 3  // Previous attempt failed; scheduled for another attempt
Completed  = 4  // Step finished successfully
Failed     = 5  // Step exhausted max retries; pipeline cannot continue
Abandoned  = 6  // Step was superseded or the submission was cancelled
```

**`SubmissionOutbox`** aggregate root (replaces `SubmissionOutboxModel`):
```csharp
public sealed class SubmissionOutbox : AggregateRoot
{
    public Guid SubmissionId       { get; }
    public SubmissionOutboxStep  Step       { get; }
    public SubmissionOutboxStatus Status    { get; private set; }
    public int  AttemptCount       { get; private set; }
    public int  MaxAttempts        { get; }          // default 5
    public DateTime  CreatedAt     { get; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? CompletedAt   { get; private set; }
    public string?   LastError     { get; private set; }

    // Domain methods
    public void RecordAttempt(DateTime now);   // Pending/Retrying → Processing
    public void Complete(DateTime now);        // Processing → Completed
    public void RecordFailure(string error, DateTime now);  // → Retrying or Failed
    public bool CanRetry { get; }             // AttemptCount < MaxAttempts
}
```

**Factory method** — the step transition creates the next row:
```csharp
public static SubmissionOutbox CreateForStep(Guid submissionId, SubmissionOutboxStep step, int maxAttempts = 5);
```

**Repository interface** (`ISubmissionOutboxRepository` — new, extracted from `ISubmissionRepository`):
```csharp
Task<IReadOnlyList<SubmissionOutbox>> GetPendingByStepAsync(SubmissionOutboxStep step, int batchSize, CancellationToken ct);
Task AddAsync(SubmissionOutbox outbox, CancellationToken ct);
Task UpdateAsync(SubmissionOutbox outbox, CancellationToken ct);
Task<IReadOnlyList<SubmissionOutbox>> GetBySubmissionIdAsync(Guid submissionId, CancellationToken ct);
```

#### Infrastructure changes (`Algowars.Infrastructure`)

**`SubmissionOutboxDataModel`** — single table `submission_outbox_steps`, columns: `id`, `submission_id`, `step` (int), `status` (int), `attempt_count`, `max_attempts`, `created_at`, `last_attempt_at`, `completed_at`, `last_error`. Replace the old `SubmissionOutboxEntity`, `SubmissionOutboxStatusEntity`, `SubmissionOutboxTypeEntity` tables — the status and step are now simple ints (no lookup tables needed).

**`SubmissionOutboxRepository`** — implements `ISubmissionOutboxRepository`. Uses `SELECT ... WHERE step = @step AND status IN (Pending, Retrying) AND (last_attempt_at IS NULL OR last_attempt_at < @now - interval)` for polling.

**Existing `ISubmissionRepository`** — remove all outbox methods (`GetSubmissionOutboxesAsync`, `IncrementOutboxesCountAsync`, `FinalizeEvaluationAsync`, etc.). Those concerns move to `ISubmissionOutboxRepository`.

#### Application layer changes (`Algowars.Application`)

Remove `IncrementSubmissionOutboxesCommand` and replace with explicit step-lifecycle commands:
- `BeginOutboxStepCommand(SubmissionOutboxStep step, Guid submissionId)` — inserts a new outbox row with status Pending
- `RecordOutboxAttemptCommand(Guid outboxId)` — transitions Pending/Retrying → Processing
- `CompleteOutboxStepCommand(Guid outboxId)` — transitions Processing → Completed; if there is a next step, dispatches `BeginOutboxStepCommand` for it
- `FailOutboxStepCommand(Guid outboxId, string error)` — transitions Processing → Retrying (if CanRetry) or Failed

Update the Quartz job and MassTransit consumers to call the new commands instead of the raw `IncrementOutboxesCountAsync` / `FinalizeEvaluationAsync` calls.

Add a `GetOutboxByStepQuery(SubmissionOutboxStep step)` that replaces `GetSubmissionOutboxesQuery` — returns only rows for the requested step.

#### EF migration

After implementing the above, add a new EF migration `AddSubmissionOutboxSteps` that creates the `submission_outbox_steps` table and drops (or renames) the three old outbox tables (`submission_outbox`, `submission_outbox_statuses`, `submission_outbox_types`).

**Done when**: The `submission_outbox_steps` table is the sole outbox persistence store; each pipeline step (Execute, PollExecution, Evaluate, EvaluationPoll) inserts a new row on entry; `AttemptCount` increments on each retry without creating a new row; job and consumer tests confirm per-step status tracking; full solution builds with zero errors and zero warnings; all unit tests pass.
