using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using PublicApi.Authorization;
using PublicApi.Filters;

namespace PublicApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOpenApi();
        services.AddControllers(options => options.Filters.Add<WrapResponseAttribute>());
        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ReportApiVersions = true;
        });

        services
            .AddAuthentication(configuration)
            .AddAuthorization()
            .AddCors(configuration)
            .AddRateLimiting();

        return services;
    }

    private static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{configuration["Auth0:Domain"]}/";
                options.Audience = configuration["Auth0:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                };
            });

        return services;
    }

    private static readonly string[] configure =
    [
        "create:problems",
        "read:admin-problems",
        "read:admin-problem",
        "read:languages",
    ];

    private static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (string? permission in configure)
            {
                options.AddPolicy(
                    permission,
                    policy => policy.Requirements.Add(new RbacRequirement(permission))
                );
            }
        });

        services.AddSingleton<IAuthorizationHandler, RbacHandler>();

        return services;
    }

    private static IServiceCollection AddCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string corsOriginsEnv =
            configuration["Cors:AllowedOrigins"]
            ?? throw new InvalidOperationException("Cors:AllowedOrigins not configured!");

        string[] allowedOrigins = corsOriginsEnv.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins cannot be empty!");
        }

        services.AddCors(options =>
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            )
        );

        return services;
    }

    private static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(
                "ExtraShort",
                opts =>
                {
                    opts.PermitLimit = 4;
                    opts.Window = TimeSpan.FromSeconds(100);
                    opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opts.QueueLimit = 2;
                }
            );

            options.AddFixedWindowLimiter(
                "Short",
                opts =>
                {
                    opts.PermitLimit = 10;
                    opts.Window = TimeSpan.FromSeconds(30);
                    opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opts.QueueLimit = 2;
                }
            );

            options.AddFixedWindowLimiter(
                "Medium",
                opts =>
                {
                    opts.PermitLimit = 20;
                    opts.Window = TimeSpan.FromMinutes(1);
                    opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opts.QueueLimit = 2;
                }
            );

            options.AddFixedWindowLimiter(
                "Long",
                opts =>
                {
                    opts.PermitLimit = 50;
                    opts.Window = TimeSpan.FromMinutes(5);
                    opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opts.QueueLimit = 5;
                }
            );

            options.AddPolicy(
                "SubmissionDaily",
                context =>
                {
                    string userId =
                        context.User.FindFirst("sub")?.Value
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 50,
                            Window = TimeSpan.FromDays(1),
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        }
                    );
                }
            );
        });

        return services;
    }
}
