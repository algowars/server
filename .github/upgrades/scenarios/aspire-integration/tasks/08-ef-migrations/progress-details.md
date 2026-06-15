## Files Modified

- `Algowars.Infrastructure/AlgoWarsDbContextFactory.cs` — new `IDesignTimeDbContextFactory<AlgoWarsDbContext>` reads `ConnectionStrings__algowars-db` env var, falls back to localhost defaults for tooling
- `Algowars.Infrastructure/Persistence/Migrations/20260615022633_InitialCreate.cs` — generated EF Core migration creating all 10 tables
- `Algowars.Infrastructure/Persistence/Migrations/20260615022633_InitialCreate.Designer.cs` — EF migration designer file
- `Algowars.Infrastructure/Persistence/Migrations/AlgoWarsDbContextModelSnapshot.cs` — EF model snapshot
- `Algowars.Api/Program.cs` — added `db.Database.MigrateAsync()` on startup; added usings for `AlgoWarsDbContext` and `Microsoft.EntityFrameworkCore`

## Tables in Migration

`users`, `problems`, `problem_versions`, `test_cases`, `code_templates`, `programming_languages`, `programming_language_versions`, `submissions`, `submission_results`, `submission_test_cases`

## Build Results

- `Algowars.Infrastructure`: 0 warnings, 0 errors
- Full solution (`Algowars.slnx`): 0 errors; 3 MSB3277 warnings in `Algowars.UnitTests` (EFCore.Relational version mismatch between 10.0.8 and 10.0.9) — deferred to Task 07

## Issues Resolved

- Initial factory used `AddEnvironmentVariables()` which required `Microsoft.Extensions.Configuration.EnvironmentVariables` not referenced in the class library — replaced with direct `Environment.GetEnvironmentVariable()` call

## Commit

`58f5cd4` — feat: add EF Core code-first migrations (Task 08)
