# Algowars Server

Backend for Algowars, a competitive programming platform. Built with .NET and Aspire.

## Requirements

- .NET 10 SDK
- Docker Desktop
- dotnet-ef CLI

```
dotnet tool install --global dotnet-ef
```

## Running the project

The project uses .NET Aspire to orchestrate the API, PostgreSQL, and RabbitMQ. Start everything by running the AppHost:

```
dotnet run --project Algowars.AppHost
```

This will spin up all dependencies and launch the Aspire dashboard.

## Migrations

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

## Postman

Base URL variable: `{{baseUrl}}` = `https://localhost:<port>`

### Users

**Create user**
```
POST {{baseUrl}}/api/v1/users
Content-Type: application/json

{
  "username": "john",
  "sub": "auth0|abc123",
  "imageUrl": "https://example.com/avatar.png"
}
```

Returns `201 Created` with the new user `Guid`, `409 Conflict` if the user already exists.

### Account

**Update account**
```
PUT {{baseUrl}}/api/v1/account
```

## Project structure

| Project | Description |
|---|---|
| `Algowars.AppHost` | Aspire orchestration |
| `Algowars.Api` | HTTP API |
| `Algowars.Application` | Application logic and handlers |
| `Algowars.Domain` | Domain models |
| `Algowars.Infrastructure` | EF Core, repositories, persistence |
| `Algowars.ServiceDefaults` | Shared Aspire service configuration |
