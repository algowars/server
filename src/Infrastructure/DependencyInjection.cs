using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Castle.Core.Configuration;
using Infrastructure.CodeExecution.Judge0;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var cs =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'."
            );

        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseNpgsql(cs, npg => npg.EnableRetryOnFailure());
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ISlugService, SlugService>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        AddJudge0Client(services, configuration);

        string runWorker = configuration.GetValue<bool>("ExecutionEngines:Judge0:RunWorker");
        if (runWorker)
        {
            services.AddHostedService<Judge0JobWorker>();
        }

        return services;
    }

    private static void AddJudge0Client(IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetSection("ExecutionEngines:Judge0").GetValue<bool>("Enabled");

        if (!enabled)
        {
            services.AddScoped<IJudge0Client, MockJudge0Client>();
            return;
        }

        var judge0Section = configuration.GetSection("Judge0");

        var baseUrl =
            judge0Section.GetValue<string>("BaseUrl")
            ?? throw new InvalidOperationException("Judge0 BaseUrl is missing.");

        var apiKey =
            judge0Section.GetValue<string>("ApiKey")
            ?? throw new InvalidOperationException("Judge0 ApiKey is missing.");

        var host =
            judge0Section.GetValue<string>("Host")
            ?? throw new InvalidOperationException("Judge0 Host is missing.");

        var timeout = judge0Section.GetValue<int?>("DefaultTimeoutInSeconds") ?? 10;

        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        services.AddHttpClient<IJudge0Client, Judge0Client>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);

            client.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
            client.DefaultRequestHeaders.Add("x-rapidapi-host", host);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }
}
