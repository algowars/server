using Algowars.Application.Settings;
using Algowars.Domain.Users;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Algowars.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOption<ConnectionStringOptions>(configuration);

        services.AddPersistence();
        services.AddRepositories();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<AlgowarsDbContext>((serviceProvider, dbOptions) =>
        {
            var options = serviceProvider
                .GetRequiredService<ConnectionStringOptions>();

            dbOptions.UseNpgsql(options.DefaultConnection);
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}