# 05-persistence-layer: Migrate Infrastructure → Algowars.Infrastructure

Move source files from `src/Infrastructure/` into `Algowars.Infrastructure/` and update all namespaces from `Infrastructure.*` to `Algowars.Infrastructure.*`.

**DbContext rename**: `AppDbContext` → `AlgoWarsDbContext`. Keep all existing entity configurations (Problems, ProblemSetups, TestSuites, Submissions, Languages). Add `Users` DbSet using a `UserDataModel` persistence model (replaces `AccountEntity`).

**Aspire DB connection**: Replace `services.AddDbContext<AppDbContext>(o => o.UseNpgsql(config["ConnectionStrings:DefaultConnection"]))` with `builder.AddNpgsqlDbContext<AlgoWarsDbContext>("algowars-db")`. Change registration method signature to accept `IHostApplicationBuilder`.

**Aspire MQ connection**: Replace manual `MessageBus:RabbitMQ:*` / `MessageBus:AzureServiceBus:*` appsettings reads with `builder.Configuration["ConnectionStrings:algowars-mq"]` (Aspire injects this automatically from AppHost).

**Repository**: Implement `IUserRepository` (replaces `IAccountRepository`). Retain `ProblemRepository` and `SubmissionRepository` with namespace updates.

**Mappings**: Update `AccountMappings` → `UserMappings` (AccountEntity → UserDataModel, AccountModel → User domain entity).

**Package update**: Replace `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL` explicit package references with `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` (bundles both + Aspire integration).

**Done when**: `Algowars.Infrastructure` builds; `IUserRepository` is implemented; `AlgoWarsDbContext` uses Aspire connection; no manual `ConnectionStrings:DefaultConnection` or `MessageBus:*` reads remain.
