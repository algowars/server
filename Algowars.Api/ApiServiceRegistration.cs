using Algowars.Api.Extensions;
using Algowars.Api.Middleware;
using Algowars.Api.Settings;
using Algowars.Infrastructure;
using Asp.Versioning;
using Scalar.AspNetCore;

namespace Algowars.Api;

public static class ApiServiceRegistration
{
    private const string CorsPolicyName = "AllowedOrigins";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
        });
        services.AddOpenApi();

        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{CorsOptions.SectionName}' is missing or invalid.");

        services.AddSingleton(corsOptions);
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddAuth0(configuration);
        services.AddAppInsights(configuration);
        services.AddScoped<AccountContextMiddleware>();

        return services;
    }

    public static async Task UseApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            await app.Services.MigrateAsync();
        }

        app.UseHttpsRedirection();
        app.UseCors(CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AccountContextMiddleware>();
        app.MapControllers();
    }
}