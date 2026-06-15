# Task 05 — Migrate Infrastructure → Algowars.Infrastructure

## What Changed

### Persistence entities (code-first, aligned to new domain)
- `Persistence/Entities/AuditableEntity.cs` — base class with CreatedOn/UpdatedOn
- `Persistence/Entities/Users/UserDataModel.cs` — replaces AccountEntity
- `Persistence/Entities/Languages/ProgrammingLanguageDataModel.cs`
- `Persistence/Entities/Languages/LanguageVersionDataModel.cs`
- `Persistence/Entities/Problems/ProblemDataModel.cs`
- `Persistence/Entities/Problems/ProblemVersionDataModel.cs`
- `Persistence/Entities/Problems/TestCaseDataModel.cs`
- `Persistence/Entities/Problems/CodeTemplateDataModel.cs`
- `Persistence/Entities/Submissions/SubmissionDataModel.cs`
- `Persistence/Entities/Submissions/SubmissionResultDataModel.cs`
- `Persistence/Entities/Submissions/SubmissionTestCaseDataModel.cs`

### DbContext
- `Persistence/AlgoWarsDbContext.cs` — replaces AppDbContext; uses `ApplyConfigurationsFromAssembly`

### Repositories
- `Repositories/UserRepository.cs` — implements `IUserRepository` (replaces AccountRepository)
- `Repositories/ProblemRepository.cs` — implements `IProblemRepository` with `GetPageAsync`
- `Repositories/SubmissionRepository.cs` — implements `ISubmissionRepository`

### Messaging
- `Messaging/Consumers/SubmissionCreatedConsumer.cs` — minimal stub; full pipeline wired in task 09

### Registration
- `InfrastructureServiceRegistration.cs` — `IHostApplicationBuilder.AddInfrastructure()`
  - `builder.AddNpgsqlDbContext<AlgoWarsDbContext>("algowars-db")` (Aspire-managed)
  - `ConnectionStrings:algowars-mq` drives RabbitMQ vs Azure Service Bus selection
  - No manual `ConnectionStrings:DefaultConnection` or `MessageBus:*` appsettings reads remain

### Domain additions
- `Algowars.Domain/SeedWork/Entity.cs` — `CreatedOn` property (added in task 04)
- `Algowars.Domain/SeedWork/PageResult.cs` — pagination envelope (added in task 04)
- `Algowars.Domain/Problems/IProblemRepository.cs` — `GetPageAsync` added (task 04)

## Build Result
- Algowars.Infrastructure: 0 errors, 0 warnings
