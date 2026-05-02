using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Messaging;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Infrastructure.CodeExecution.Judge0;
using Infrastructure.Configuration;
using Infrastructure.Jobs;
using Infrastructure.Jobs.JobHandlers;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using System.Text.Json;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions(configuration)
            .AddPersistence(configuration)
            .AddRepositories()
            .AddServices()
            .AddMessageBus(configuration)
            .AddJudge0Client(configuration)
            .AddJobs();

        return services;
    }

    private static IServiceCollection AddOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<ConnectionStringOptions>()
            .Bind(configuration.GetSection(ConnectionStringOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.DefaultConnection),
                "DefaultConnection connection string is required"
            )
            .ValidateOnStart();

        services
            .AddOptions<ExecutionEnginesOptions>()
            .Bind(configuration.GetSection("ExecutionEngines"))
            .ValidateOnStart();

        services.AddSingleton(
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
            }
        );

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(
            (sp, o) =>
            {
                var cs = sp.GetRequiredService<IOptions<ConnectionStringOptions>>().Value;
                o.UseNpgsql(cs.DefaultConnection);
            }
        );

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ISlugService, SlugService>();

        return services;
    }

    private static IServiceCollection AddMessageBus(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<MessageBusOptions>()
            .Bind(configuration.GetSection(MessageBusOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<IMessagePublisher, MassTransitMessagePublisher>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<SubmissionCreatedConsumer>();
            bus.AddConsumer<SubmissionExecutedConsumer>();
            bus.AddConsumer<SubmissionReadyToEvaluateConsumer>();
            bus.AddConsumer<SubmissionEvaluationPollConsumer>();

            string transport = configuration
                .GetSection(MessageBusOptions.SectionName)
                .GetValue<string>("Transport") ?? "RabbitMQ";

            if (transport.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase))
            {
                bus.UsingAzureServiceBus((ctx, cfg) =>
                {
                    var opts = ctx.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                    cfg.Host(opts.AzureServiceBus.ConnectionString);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                bus.UsingRabbitMq((ctx, cfg) =>
                {
                    var opts = ctx.GetRequiredService<IOptions<MessageBusOptions>>().Value;
                    cfg.Host(opts.RabbitMQ.Host, opts.RabbitMQ.VirtualHost, h =>
                    {
                        h.Username(opts.RabbitMQ.Username);
                        h.Password(opts.RabbitMQ.Password);
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.AddJobAndTrigger<SubmissionExecutionHandler>(JobType.SubmissionExecution, intervalInMinutes: 60);
            q.AddJobAndTrigger<PollSubmissionExecutionHander>(JobType.PollSubmissionExecution, intervalInMinutes: 60);
            q.AddJobAndTrigger<EvaluateSubmissionHandler>(JobType.EvaluateSubmission, intervalInMinutes: 60);
            q.AddJobAndTrigger<PollEvaluationHandler>(JobType.PollEvaluation, intervalInMinutes: 60);
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }

    private static void AddJobAndTrigger<T>(this IServiceCollectionQuartzConfigurator q, JobType jobType, int intervalInMinutes)
        where T : IJob
    {
        string jobName = jobType.ToString();
        var jobKey = new JobKey(jobName);

        q.AddJob<T>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobName}-trigger")
            .WithSimpleSchedule(s => s
                .WithIntervalInMinutes(intervalInMinutes)
                .RepeatForever()
            )
        );
    }

    private static IServiceCollection AddJudge0Client(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient<IJudge0Client, Judge0Client>(
            (serviceProvider, client) =>
            {
                var judge0 = serviceProvider
                    .GetRequiredService<IOptions<ExecutionEnginesOptions>>()
                    .Value.Judge0;

                string baseUrl = judge0.BaseUrl.EndsWith('/')
                    ? judge0.BaseUrl
                    : judge0.BaseUrl + "/";

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(judge0.DefaultTimeoutInSeconds);

                client.DefaultRequestHeaders.Add("x-rapidapi-host", judge0.Host);
                client.DefaultRequestHeaders.Add("x-rapidapi-key", judge0.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }
        );

        return services;
    }
}