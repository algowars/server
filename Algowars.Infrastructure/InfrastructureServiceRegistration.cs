using Algowars.Application.Messaging;
using Algowars.Domain.Problems;
using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Outbox;
using Algowars.Domain.Users;
using Algowars.Infrastructure.Messaging.Consumers;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Algowars.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AlgoWarsDbContext>("algowars-db");

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
        builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        builder.Services.AddScoped<ISubmissionOutboxRepository, SubmissionOutboxRepository>();

        builder.Services.AddMassTransit(bus =>
        {
            bus.AddConsumer<SubmissionCreatedConsumer>();

            var connectionString = builder.Configuration["ConnectionStrings:algowars-mq"];

            if (!string.IsNullOrWhiteSpace(connectionString) &&
                connectionString.StartsWith("amqps://", StringComparison.OrdinalIgnoreCase))
            {
                bus.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                bus.UsingRabbitMq((ctx, cfg) =>
                {
                    if (!string.IsNullOrWhiteSpace(connectionString))
                        cfg.Host(new Uri(connectionString));
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        return builder;
    }
}
