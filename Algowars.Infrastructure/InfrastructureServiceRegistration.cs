using Algowars.Domain.Users;
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

        builder.Services.AddMassTransit(x =>
        {
            if (builder.Environment.IsDevelopment())
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(builder.Configuration["ConnectionStrings:algowars-mq"]);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                x.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.Host(builder.Configuration["ConnectionStrings:algowars-mq"]);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        return builder;
    }
}
