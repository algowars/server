using System.Text.Json;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Infrastructure.CodeExecution.Judge0;
using Infrastructure.Configuration;
using Infrastructure.Jobs;
using Infrastructure.Jobs.JobHandlers;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

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
            .AddConfiguredBackgroundJobs(configuration)
            .AddJudge0Client(configuration);

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

    private static IServiceCollection AddConfiguredBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jobsSection = configuration.GetSection("BackgroundJobs");

        bool GetEnabled(string jobKey, bool defaultEnabled) =>
            jobsSection.GetValue<bool?>($"{jobKey}:Enabled") ?? defaultEnabled;

        TimeSpan GetInterval(string jobKey, double defaultSeconds) =>
            TimeSpan.FromSeconds(
                jobsSection.GetValue<double?>($"{jobKey}:IntervalSeconds") ?? defaultSeconds
            );

        services.AddQuartz(q =>
        {
            q.AddJobWithTrigger<SubmissionExecutionHandler>(
                JobType.SubmissionExecution,
                GetInterval("SubmissionExecution", 5),
                GetEnabled("SubmissionExecution", false)
            );

            q.AddJobWithTrigger<PollExecutionHandler>(
                JobType.PollExecution,
                GetInterval("PollExecution", 10),
                GetEnabled("PollExecution", false)
            );

            q.AddJobWithTrigger<SubmissionEvaluatorHandler>(
                JobType.SubmissionEvaluator,
                GetInterval("SubmissionEvaluator", 5),
                GetEnabled("SubmissionEvaluator", false)
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }

    private static void AddJobWithTrigger<TJob>(
        this IServiceCollectionQuartzConfigurator q,
        JobType jobType,
        TimeSpan interval,
        bool enabled
    )
        where TJob : IJob
    {
        if (!enabled)
        {
            return;
        }

        var jobKey = new JobKey(jobType.ToString());

        q.AddJob<TJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts =>
            opts.ForJob(jobKey)
                .WithIdentity($"{jobType}-trigger")
                .WithSimpleSchedule(s => s.WithInterval(interval).RepeatForever())
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

                client.DefaultRequestHeaders.Add("x-rapidapi-key", judge0.ApiKey);
                client.DefaultRequestHeaders.Add("x-rapidapi-host", judge0.Host);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }
        );

        return services;
    }
}
