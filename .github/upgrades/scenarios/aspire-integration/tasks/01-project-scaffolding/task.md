# 01-project-scaffolding: Set up new project structure at root

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
