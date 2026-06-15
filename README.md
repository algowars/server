# Algowars Server

[![ci](https://github.com/algowars/server/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/algowars/server/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=algowars_server&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=algowars_server)
[![codecov](https://codecov.io/gh/algowars/server/branch/master/graph/badge.svg)](https://codecov.io/gh/algowars/server)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Postgres + RabbitMQ containers)
- [Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling): `dotnet workload install aspire`

## Running locally

```powershell
dotnet run --project Algowars.AppHost
```

Aspire starts Postgres and RabbitMQ in Docker, applies all pending EF migrations, and launches the API. The Aspire dashboard opens at `http://localhost:15888`.

## Configuration

All secrets are stored in [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) on the `Algowars.Api` project.

```powershell
cd Algowars.Api
dotnet user-secrets set "Auth0:Authority"   "https://<your-tenant>.auth0.com/"
dotnet user-secrets set "Auth0:Audience"    "<your-api-identifier>"
```

### CORS

In development, `http://localhost:3000`, `http://localhost:4200`, and `http://localhost:5173` are allowed by default (`appsettings.Development.json`). Add more origins there or via user secrets:

```powershell
dotnet user-secrets set "Cors:AllowedOrigins:0" "http://localhost:3000"
```

In production set `Cors:AllowedOrigins` to your deployed frontend URL(s).

### Message bus

Aspire wires RabbitMQ automatically in dev. For production set `ConnectionStrings:algowars-mq` to an Azure Service Bus connection string (starts with `amqps://`).
