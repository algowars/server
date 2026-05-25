# Algowars Server

[![ci](https://github.com/algowars/server/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/algowars/server/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=algowars_server&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=algowars_server)
[![codecov](https://codecov.io/gh/algowars/server/branch/master/graph/badge.svg)](https://codecov.io/gh/algowars/server)

## Local user secrets setup

The scripts below populate user secrets for `src/PublicApi/PublicApi.csproj`.
CORS is configured in `src/PublicApi/appsettings.Development.json`.

### PowerShell (Windows)

From the repository root:

```powershell
./scripts/setup-user-secrets.ps1
```

### Bash (Linux/macOS/Git Bash)

From the repository root:

```bash
./scripts/setup-user-secrets.sh
```

### Prompt behavior

- Press `Enter` to use the shown default value.
- Type `skip` to leave a key unchanged.
- Set `MessageBus:Transport` to `RabbitMQ` or `AzureServiceBus`.
  - `RabbitMQ`: prompts only RabbitMQ settings.
  - `AzureServiceBus`: prompts only Azure Service Bus connection string.
