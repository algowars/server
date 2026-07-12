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

### Seeding

Seed everything (languages + demo problems with tags):

```
dotnet run --project Algowars.Seeder -- --all
```

Seed only static data (languages):

```
dotnet run --project Algowars.Seeder -- --static
```

Seed only demo data (problems, test suites, tags):

```
dotnet run --project Algowars.Seeder -- --demo
```

### Deployment environments

The deployment workflows use GitHub Environments named `Scrum` and `Production`.
Each environment should define the same settings, with values appropriate for that target.

Environment variables:

- `APP_URL`: Public URL for the deployed API. Used for the GitHub deployment environment link.
- `CORS_ALLOWED_ORIGINS`: Comma-separated list of allowed frontend origins.
- `DEPLOY_SCRIPT_PATH`: Repo-relative path to the deployment script to execute, for example `scripts/deploy-scrum.sh`.
- `MESSAGEBUS_TRANSPORT`: `RabbitMQ` or `AzureServiceBus`.
- `QUARTZ_SUBMISSIONCLEANUPJOB_CRONEXPRESSION`: Cron expression for the submission cleanup job.

Environment secrets:

- `AUTH0_AUDIENCE`: Auth0 API audience.
- `AUTH0_DOMAIN`: Auth0 domain.
- `AZURE_SERVICEBUS_CONNECTION_STRING`: Required when `MESSAGEBUS_TRANSPORT` is `AzureServiceBus`.
- `DB_CONNECTION_STRING`: Database connection string used for EF Core migrations and app runtime configuration.
- `RABBITMQ_HOST`: Required when `MESSAGEBUS_TRANSPORT` is `RabbitMQ`.
- `RABBITMQ_PASSWORD`: Required when `MESSAGEBUS_TRANSPORT` is `RabbitMQ`.
- `RABBITMQ_USERNAME`: Required when `MESSAGEBUS_TRANSPORT` is `RabbitMQ`.
- `RABBITMQ_VIRTUALHOST`: Required when `MESSAGEBUS_TRANSPORT` is `RabbitMQ`.
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Optional, but recommended for production telemetry.

The deploy workflows publish the API, generate an EF migrations bundle, run the migrations against `DB_CONNECTION_STRING`, and then execute `DEPLOY_SCRIPT_PATH`.
The deployment script receives the following environment variables:

- `APP_URL`
- `ASPNETCORE_ENVIRONMENT`
- `AUTH0_AUDIENCE`
- `AUTH0_DOMAIN`
- `AZURE_SERVICEBUS_CONNECTION_STRING`
- `CORS_ALLOWED_ORIGINS`
- `DB_CONNECTION_STRING`
- `MESSAGEBUS_TRANSPORT`
- `MIGRATION_BUNDLE_PATH`
- `PUBLISH_DIR`
- `QUARTZ_SUBMISSIONCLEANUPJOB_CRONEXPRESSION`
- `RABBITMQ_HOST`
- `RABBITMQ_PASSWORD`
- `RABBITMQ_USERNAME`
- `RABBITMQ_VIRTUALHOST`
- `APPLICATIONINSIGHTS_CONNECTION_STRING`

### Project structure

| Project                    | Description                         |
| -------------------------- | ----------------------------------- |
| `Algowars.AppHost`         | Aspire orchestration                |
| `Algowars.Api`             | HTTP API                            |
| `Algowars.Application`     | Application logic and handlers      |
| `Algowars.Domain`          | Domain models                       |
| `Algowars.Infrastructure`  | EF Core, repositories, persistence  |
| `Algowars.ServiceDefaults` | Shared Aspire service configuration |
