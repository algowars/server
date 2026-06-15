# Task 04 — Migrate ApplicationCore → Algowars.Application

## What Changed

### New project: Algowars.Application
Packages: Ardalis.Result, FluentValidation, FluentValidation.DependencyInjectionExtensions, MassTransit, Mapster, MediatR.

### Commands
- `Commands/ICommand.cs` — `ICommand<TResponse>` and `ICommand` MediatR request abstractions
- `Commands/ICommandHandler.cs` — typed handler interfaces
- `Commands/AbstractCommandHandler.cs` — validates via FluentValidation, maps failures to `Result.Invalid`
- `Commands/Users/CreateUser/` — CreateUserCommand, CreateUserHandler, CreateUserValidator
- `Commands/Submissions/CreateSubmission/` — CreateSubmissionCommand, CreateSubmissionHandler, CreateSubmissionValidator

### Queries
- `Queries/IQuery.cs` / `IQueryHandler.cs` — query/handler interfaces
- `Queries/Users/GetUserBySub/` — GetUserBySubQuery + Handler
- `Queries/Users/GetProfileAggregate/` — GetProfileAggregateQuery + Handler
- `Queries/Users/GetProfileSettings/` — GetProfileSettingsQuery + Handler
- `Queries/Problems/GetProblemsPageable/` — paginated problem listing
- `Queries/Problems/GetProblemBySlug/` — single problem by slug
- `Queries/Submissions/GetSubmissionStatus/` — submission result polling

### Services
- `Services/Users/IUserService.cs` + `UserService.cs` — delegates to MediatR

### Common
- `Common/Pagination/SortDirection.cs`, `PaginationRequest.cs`, `PaginatedResult.cs`

### DTOs
- Users: UserDto, ProfileAggregateDto, ProfileSettingsDto
- Problems: ProblemDto, ProblemSubmissionDto
- Languages: LanguageDto, LanguageVersionDto
- Submissions: SubmissionDto, SubmissionResultDto, SubmissionStatusDto

### Messaging
- `Messaging/SubmissionCreatedMessage.cs`

### Registration
- `ApplicationServiceRegistration.cs` — wires MediatR, FluentValidation, UserFactory, IUserService

### Domain additions to support Application layer
- `Algowars.Domain/SeedWork/Entity.cs` — added `CreatedOn` property
- `Algowars.Domain/SeedWork/PageResult.cs` — new pagination envelope
- `Algowars.Domain/Problems/IProblemRepository.cs` — added `GetPageAsync`

## Build Result
- Algowars.Domain: 0 errors, 0 warnings
- Algowars.Application: 0 errors, 0 warnings
