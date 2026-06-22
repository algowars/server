using Algowars.Application.Problems;
using Algowars.Application.Users;
using Algowars.Application.Settings;
using Algowars.Domain.TestSuites;
using Algowars.Domain.Users;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Seeders;
using Algowars.Infrastructure.Repositories;
using Algowars.Infrastructure.Settings;
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
        services.AddSeeders();

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
        services.AddScoped<IUserWriteRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IProblemReadRepository, ProblemReadRepository>();
        services.AddScoped<ITestSuiteRepository, TestSuiteRepository>();

        return services;
    }

    public static async Task MigrateAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AlgowarsDbContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task SeedAsync(this IServiceProvider services, SeederOptions options, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        if (options.SeedStaticData)
        {
            var languageSeeder = scope.ServiceProvider.GetRequiredService<LanguageSeeder>();
            await languageSeeder.SeedAsync(cancellationToken);
        }

        if (options.SeedDemoData)
        {
            var demoSeeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
            await demoSeeder.SeedAsync(cancellationToken);
        }
    }
}