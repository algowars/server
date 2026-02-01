using System.Text.Json;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Jobs;
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

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
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

        services
            .AddOptions<ExecutionEnginesOptions>()
            .Bind(configuration.GetSection("ExecutionEngines"))
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>(
            (sp, o) =>
            {
                var cs = sp.GetRequiredService<IOptions<ConnectionStringOptions>>().Value;
                o.UseNpgsql(cs.DefaultConnection);
            }
        );

        services.AddScoped<ISlugService, SlugService>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        services.AddScoped<SubmissionInitializerHandler>();

        var jobsSection = configuration.GetSection("BackgroundJobs");

        bool GetEnabled(string jobKey, bool defaultEnabled) =>
            jobsSection.GetValue<bool?>($"{jobKey}:Enabled") ?? defaultEnabled;

        TimeSpan GetInterval(string jobKey, double defaultSeconds) =>
            TimeSpan.FromSeconds(
                jobsSection.GetValue<double?>($"{jobKey}:IntervalSeconds") ?? defaultSeconds
            );

        services.AddBackgroundJobs(jobs =>
        {
            jobs.Register<SubmissionInitializerHandler>(
                jobType: BackgroundJobType.SubmissionInitializer,
                interval: GetInterval("SubmissionInitializer", 5),
                enabled: GetEnabled("SubmissionInitializer", false)
            );
        });

        AddJudge0Client(services, configuration);

        return services;
    }

    private static void AddJudge0Client(IServiceCollection services, IConfiguration configuration)
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
    }

    private static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        Action<JobRegistry> configure
    )
    {
        var registry = new JobRegistry();
        configure(registry);

        services.AddSingleton(registry);
        services.AddSingleton<JobRunner>();
        services.AddHostedService<BackgroundJobService>();

        return services;
    }
}
