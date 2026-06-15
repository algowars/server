# 07-test-project: Migrate UnitTests → Algowars.UnitTests

Move test files from `tests/UnitTests/` into `Algowars.UnitTests/` and update all namespaces from the old `ApplicationCore.*` / `Infrastructure.*` / `PublicApi.*` patterns to the new `Algowars.*` equivalents.

Rename account-related test files and classes:
- `AccountModelTests.cs` → `UserTests.cs` (updated to test the new DDD `User` entity)
- `GetAccountBySubHandlerTests.cs` → `GetUserBySubHandlerTests.cs`
- `GetProfileAggregateHandlerTests.cs` → update Account references
- `GetProfileSettingsHandlerTests.cs` → update Account references

Update project references to `Algowars.Application`, `Algowars.Infrastructure`, `Algowars.Api`.

**Done when**: `Algowars.UnitTests` builds; all tests pass; no old namespace references remain.
