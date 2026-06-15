# 04-application-layer: Migrate ApplicationCore → Algowars.Application

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
