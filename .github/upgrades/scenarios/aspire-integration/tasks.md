# Aspire Integration + Modernization Progress

## Overview

Restructuring the Algowars Server from a `src/`+`tests/` layout to `Algowars.*` projects at repo root. Introduces a pure DDD `Algowars.Domain` project, Aspire orchestration (AppHost + ServiceDefaults), replaces all appsettings-based DB/MQ config with Aspire-managed connections, renames the Account domain to User, and adopts EF Core code-first migrations.

**Progress**: 6/9 tasks complete <progress value="67" max="100"></progress> 67%

## Tasks

- ✅ 01-project-scaffolding: Set up new project structure at root ([Content](tasks/01-project-scaffolding/task.md), [Progress](tasks/01-project-scaffolding/progress-details.md))
- ✅ 02-domain-layer: Create Algowars.Domain ([Content](tasks/02-domain-layer/task.md), [Progress](tasks/02-domain-layer/progress-details.md))
- ✅ 03-aspire-setup: Create Algowars.ServiceDefaults and Algowars.AppHost ([Content](tasks/03-aspire-setup/task.md), [Progress](tasks/03-aspire-setup/progress-details.md))
- ✅ 04-application-layer: Migrate ApplicationCore → Algowars.Application ([Content](tasks/04-application-layer/task.md), [Progress](tasks/04-application-layer/progress-details.md))
- ✅ 05-persistence-layer: Migrate Infrastructure → Algowars.Infrastructure ([Content](tasks/05-persistence-layer/task.md), [Progress](tasks/05-persistence-layer/progress-details.md))
- ✅ 06-api-layer: Migrate PublicApi → Algowars.Api ([Content](tasks/06-api-layer/task.md), [Progress](tasks/06-api-layer/progress-details.md))
- 🔲 07-test-project: Migrate UnitTests → Algowars.UnitTests
- 🔲 08-ef-migrations: Add EF Core code-first migration
- 🔲 09-submission-outbox-redesign: Per-step outbox ledger with retry tracking
