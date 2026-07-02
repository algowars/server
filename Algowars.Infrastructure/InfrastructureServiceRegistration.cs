using Algowars.Application.Configuration;
using Algowars.Application.Jobs.Submissions;
using Algowars.Application.Languages;
using Algowars.Application.Messaging;
using Algowars.Application.Problems;
using Algowars.Application.Settings;
using Algowars.Application.Users;
using Algowars.Domain.Submissions;
using Algowars.Domain.TestSuites;
using Algowars.Domain.Users;
using Algowars.Infrastructure.Jobs.Submissions;
using Algowars.Infrastructure.Messaging;
using Algowars.Infrastructure.Messaging.Consumers;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Seeders;
using Algowars.Infrastructure.Repositories;
using Algowars.Infrastructure.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Algowars.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions(configuration);

        services.AddPersistence();
        services.AddRepositories();

        services.AddMessageBus(configuration);
        services.AddJobs(configuration);

        services.AddSeeder();

        return services;
    }


    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessagePublisher, MassTransitMessagePublisher>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<SubmissionCreatedConsumer>();

            var opts = configuration
                .GetSection(MessageBusOptions.SectionName)
                .Get<MessageBusOptions>() ?? new MessageBusOptions();

            if (opts.Transport.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase))
            {
                bus.UsingAzureServiceBus((ctx, cfg) =>
                {
                    var busOpts = ctx.GetRequiredService<MessageBusOptions>();
                    cfg.Host(busOpts.AzureServiceBus.ConnectionString);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                bus.UsingRabbitMq((ctx, cfg) =>
                {
                    var busOpts = ctx.GetRequiredService<MessageBusOptions>();
                    cfg.Host(busOpts.RabbitMQ.Host, busOpts.RabbitMQ.VirtualHost, h =>
                    {
                        h.Username(busOpts.RabbitMQ.Username);
                        h.Password(busOpts.RabbitMQ.Password);
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISubmissionCleanupService, SubmissionCleanupService>();

        var opts = configuration
            .GetSection(JobScheduleOptions.SectionName)
            .Get<JobScheduleOptions>() ?? new JobScheduleOptions();

        services.AddQuartz(q =>
        {
            q.AddJob<SubmissionCleanupJob>(SubmissionCleanupJob.Key, j => j.StoreDurably());
            q.AddTrigger(t => t
                .ForJob(SubmissionCleanupJob.Key)
                .WithIdentity("SubmissionCleanupJob-trigger")
                .WithCronSchedule(opts.SubmissionCleanupJob.CronExpression));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOption<ConnectionStringOptions>(configuration);
        services.AddOption<MessageBusOptions>(configuration);

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
        services.AddScoped<ILanguageReadRepository, LanguageReadRepository>();
        services.AddScoped<IProblemReadRepository, ProblemReadRepository>();
        services.AddScoped<ISubmissionWriteRepository, SubmissionWriteRepository>();
        services.AddScoped<ITestSuiteWriteRepository, TestSuiteWriteRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();

        return services;
    }

    private static IServiceCollection AddSeeder(this IServiceCollection services)
    {
        services.AddScoped<LanguageSeeder>();
        services.AddScoped<DemoDataSeeder>();
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