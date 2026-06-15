# 08-ef-migrations: Add EF Core code-first migration

Add a design-time `IDesignTimeDbContextFactory<AlgoWarsDbContext>` to `Algowars.Infrastructure` so EF tooling can create migrations without a running app.

Run `dotnet ef migrations add InitialCreate --project Algowars.Infrastructure --startup-project Algowars.Api` to generate the initial migration covering the full current schema (Users, Problems, ProblemSetups, Submissions, TestSuites, Languages, etc.).

Remove any residual references to old manual SQL migration scripts.

Validate the full solution build is clean (all 7 projects, zero errors, zero warnings). Run the test suite (`dotnet test Algowars.UnitTests`) and confirm all tests pass.

**Done when**: A valid `Migrations/` folder exists in `Algowars.Infrastructure`; the migration applies cleanly against an empty PostgreSQL database; full solution builds with zero errors and zero warnings; all unit tests pass.
