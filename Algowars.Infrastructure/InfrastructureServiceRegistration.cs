using Algowars.Application.Configuration;
using Algowars.Application.ExecutionEngine;
using Algowars.Application.Jobs.Submissions;
using Algowars.Application.Languages;
using Algowars.Application.Messaging;
using Algowars.Application.Problems;
using Algowars.Application.Settings;
using Algowars.Application.Submissions;
using Algowars.Application.Users;
using Algowars.Domain.Authorization;
using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.Problems;
using Algowars.Domain.SubmissionJobs;
using Algowars.Domain.Submissions;
using Algowars.Domain.TestSuites;
using Algowars.Domain.Users;
using Algowars.Infrastructure.ExecutionEngine.CodeTemplates;
using Algowars.Infrastructure.ExecutionEngine.Judge0;
using Algowars.Infrastructure.ExecutionEngine.StepHandlers;
using Algowars.Infrastructure.Jobs.Submissions;
using Algowars.Infrastructure.Messaging;
using Algowars.Infrastructure.Messaging.Consumers;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Seeders;
using Algowars.Infrastructure.Persistence.Seeders.Problems;
using Algowars.Infrastructure.Repositories;
using Algowars.Infrastructure.Settings;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using RabbitMQ.Client;

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
        services.AddExecutionEngine(configuration);

        services.AddMessageBus(configuration);
        services.AddJobs(configuration);

        services.AddSeeder();

        return services;
    }

    /// <summary>
    /// Minimal registration for the Seeder CLI — persistence only, no message bus or jobs.
    /// </summary>
    public static IServiceCollection AddInfrastructureForSeeder(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOption<ConnectionStringOptions>(configuration);
        services.AddPersistence();
        services.AddSeeder();

        return services;
    }

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        var opts = configuration
            .GetSection(MessageBusOptions.SectionName)
            .Get<MessageBusOptions>() ?? new MessageBusOptions();

        if (opts.Transport.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton(new ServiceBusClient(opts.AzureServiceBus.ConnectionString));
            services.AddScoped<IMessagePublisher, AzureServiceBusMessagePublisher>();
            services.AddHostedService<AzureServiceBusConsumerService>();
        }
        else
        {
            services.AddSingleton<IConnection>(_ =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = opts.RabbitMQ.Host,
                    VirtualHost = opts.RabbitMQ.VirtualHost,
                    UserName = opts.RabbitMQ.Username,
                    Password = opts.RabbitMQ.Password,
                    DispatchConsumersAsync = true,
                };
                return factory.CreateConnection();
            });
            services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();
            services.AddHostedService<RabbitMqConsumerService>();
        }

        return services;
    }

    private static IServiceCollection AddExecutionEngine(this IServiceCollection services, IConfiguration configuration)
    {
        var opts = configuration
            .GetSection(ExecutionEngineOptions.SectionName)
            .Get<ExecutionEngineOptions>() ?? new ExecutionEngineOptions();

        if (opts.Judge0.Enabled)
        {
            var judge0Opts = opts.Judge0;
            services.AddHttpClient(nameof(Judge0ExecutionEngineStrategy), client =>
            {
                client.BaseAddress = new Uri(judge0Opts.BaseUrl.TrimEnd('/') + "/");
                if (!string.IsNullOrWhiteSpace(judge0Opts.ApiKey))
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", judge0Opts.ApiKey);
                if (!string.IsNullOrWhiteSpace(judge0Opts.Host))
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", judge0Opts.Host);
            });
            services.AddScoped<IExecutionEngineStrategy>(sp =>
            {
                var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
                var http = httpFactory.CreateClient(nameof(Judge0ExecutionEngineStrategy));
                return new Judge0ExecutionEngineStrategy(http, judge0Opts);
            });
        }

        // Code template strategies
        services.AddScoped<ICodeTemplateStrategy, JavaScriptCodeTemplateStrategy>();
        services.AddScoped<ICodeTemplateStrategy, TypeScriptCodeTemplateStrategy>();
        services.AddScoped<ICodeTemplateStrategy, PythonCodeTemplateStrategy>();
        services.AddScoped<ICodeTemplateStrategyResolver, CodeTemplateStrategyResolver>();

        // Step handlers and registry
        services.AddScoped<IStepHandler, Judge0ExecutionStepHandler>();
        services.AddScoped<IStepHandler, Judge0PollStepHandler>();
        services.AddScoped<IStepHandler, EvaluateStepHandler>();
        services.AddScoped<IStepHandlerRegistry, StepHandlerRegistry>();

        // Processor service (scoped — uses DbContext)
        services.AddSingleton<SubmissionJobProcessorService>();

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

            q.AddJob<SubmissionJobProcessorJob>(SubmissionJobProcessorJob.Key, j => j.StoreDurably());
            q.AddTrigger(t => t
                .ForJob(SubmissionJobProcessorJob.Key)
                .WithIdentity("SubmissionJobProcessorJob-trigger")
                .WithCronSchedule(opts.SubmissionJobProcessorJob.CronExpression));
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
        services.AddScoped<IAuthorizationReadRepository, AuthorizationReadRepository>();
        services.AddScoped<ILanguageReadRepository, LanguageReadRepository>();
        services.AddScoped<IProblemReadRepository, ProblemReadRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<ISubmissionWriteRepository, SubmissionWriteRepository>();
        services.AddScoped<ISubmissionReadRepository, SubmissionReadRepository>();
        services.AddScoped<ISubmissionJobRepository, SubmissionJobRepository>();
        services.AddScoped<IExecutionPipelineRepository, ExecutionPipelineRepository>();
        services.AddScoped<ITestSuiteWriteRepository, TestSuiteWriteRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();

        return services;
    }

    private static IServiceCollection AddSeeder(this IServiceCollection services)
    {
        services.AddScoped<AuthorizationSeeder>();
        services.AddScoped<Judge0PipelineSeeder>();
        services.AddScoped<LanguageSeeder>();
        services.AddScoped<TwoSumProblemSeeder>();
        services.AddScoped<HelloOrGoodbyeProblemSeeder>();
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
        var db = scope.ServiceProvider.GetRequiredService<AlgowarsDbContext>();

        if (options.SeedStaticData)
        {
            var authorizationSeeder = scope.ServiceProvider.GetRequiredService<AuthorizationSeeder>();
            await authorizationSeeder.SeedAsync(cancellationToken);
            db.ChangeTracker.Clear();

            var pipelineSeeder = scope.ServiceProvider.GetRequiredService<Judge0PipelineSeeder>();
            await pipelineSeeder.SeedAsync(cancellationToken);
            db.ChangeTracker.Clear();

            var languageSeeder = scope.ServiceProvider.GetRequiredService<LanguageSeeder>();
            await languageSeeder.SeedAsync(cancellationToken);
            db.ChangeTracker.Clear();

            var twoSumSeeder = scope.ServiceProvider.GetRequiredService<TwoSumProblemSeeder>();
            await twoSumSeeder.SeedAsync(cancellationToken);
            db.ChangeTracker.Clear();

            var helloOrGoodbyeSeeder = scope.ServiceProvider.GetRequiredService<HelloOrGoodbyeProblemSeeder>();
            await helloOrGoodbyeSeeder.SeedAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }

        if (options.SeedDemoData)
        {
            var demoSeeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
            await demoSeeder.SeedAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }
    }
}