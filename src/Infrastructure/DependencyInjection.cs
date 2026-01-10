using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Infrastructure.CodeExecution.Judge0;
using Infrastructure.Configuration;
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
        string cs =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'."
            );
        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseNpgsql(cs, npg => npg.EnableRetryOnFailure());
        });

        services.AddScoped<ISlugService, SlugService>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        services.Configure<ExecutionEnginesOptions>(configuration.GetSection("ExecutionEngines"));

        AddJudge0Client(services);

        return services;
    }

    private static void AddJudge0Client(IServiceCollection services)
    {
        services.AddSingleton<IJudge0Client>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ExecutionEnginesOptions>>().Value;
            var judge0 = options.Judge0;

            //if (!judge0.Enabled)
            //{
            //    return new MockJudge0Client();
            //}

            string baseUrl = judge0.BaseUrl.EndsWith("/") ? judge0.BaseUrl : judge0.BaseUrl + "/";

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(judge0.DefaultTimeoutInSeconds),
            };

            client.DefaultRequestHeaders.Add("x-rapidapi-key", judge0.ApiKey);
            client.DefaultRequestHeaders.Add("x-rapidapi-host", judge0.Host);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return new Judge0Client(client);
        });
    }
}
