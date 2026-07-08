# Algowars Server

Backend for Algowars, a competitive programming platform. Built with .NET.

## Requirements

- .NET 10 SDK
- docker
- dotnet-ef CLI

```
dotnet tool install --global dotnet-ef
```

### Getting started

To get started with the project. You need a postgresql server and a message broker. You can use the docker compose file to spin this up. To do so run the command:

```
docker-compose up
```

### Migrations

Migrations are managed in `Algowars.Infrastructure`. To add a new migration:

```
dotnet ef migrations add <MigrationName> --project Algowars.Infrastructure --startup-project Algowars.Api
```

To apply migrations against the local database (requires Aspire to be running):

```
dotnet ef database update --project Algowars.Infrastructure --startup-project Algowars.Api
```

To apply migrations against an external database:

```
dotnet ef database update --project Algowars.Infrastructure --connection "<connection string>"
```

### Project structure

| Project                    | Description                         |
| -------------------------- | ----------------------------------- |
| `Algowars.AppHost`         | Aspire orchestration                |
| `Algowars.Api`             | HTTP API                            |
| `Algowars.Application`     | Application logic and handlers      |
| `Algowars.Domain`          | Domain models                       |
| `Algowars.Infrastructure`  | EF Core, repositories, persistence  |
| `Algowars.ServiceDefaults` | Shared Aspire service configuration |
