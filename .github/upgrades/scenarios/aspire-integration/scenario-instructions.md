# Aspire Integration + Restructuring

## Strategy
Restructure solution to Algowars.* layout at repo root, introduce DDD Domain project, add Aspire orchestration (AppHost + ServiceDefaults), replace appsettings DB/MQ config with Aspire connection strings, and adopt EF Core code-first migrations.

## Preferences
- **Flow Mode**: Guided
- **Commit Strategy**: After Each Phase
- **Integration Mode**: Inner-loop + Azure-ready
- **Reference Branch**: admclamb/final-housekeeping

## Decisions
- Account domain renamed to User throughout — AccountModel, IAccountRepository, AccountController, AccountAppService all become User equivalents
- EF Core code-first migrations replace old manual SQL migrations; existing DB can be discarded (app not in production)
- DB connection: Aspire AddNpgsqlDbContext<AlgoWarsDbContext>("algowars-db") replaces manual ConnectionStrings:DefaultConnection
- Message bus connection: Aspire ConnectionStrings:algowars-mq replaces manual MessageBus:* appsettings block (dev=RabbitMQ, prod=AzureServiceBus)
- Auth0, CORS, Judge0, and ExecutionEngines settings remain in appsettings (not Aspire-managed)
- Central package management via Directory.Packages.props from reference branch
- Projects move to repo root — no src/ or tests/ subdirectories
- Submission outbox redesigned as append-only per-step ledger (task 09, done last): each pipeline step inserts a new SubmissionOutbox row; AttemptCount and Status are per-row; old lookup tables (submission_outbox_statuses, submission_outbox_types) removed; new ISubmissionOutboxRepository extracted from ISubmissionRepository

## User Preferences
### Execution Style
- Skip task 07 (test project migration) — do tests last, after all other tasks are complete
- No code comments in generated files

## Source Control
- **Source Branch**: master
- **Working Branch**: aspire-integration
- **Commit Strategy**: After Each Phase
- **Branch Sync**: Auto (Merge)
