using System;
using System.Linq;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using PublicApi.Attributes;

namespace PublicApi;

public static class RateLimitRegistrationExtensions
{
    public static IServiceCollection RegisterAllUserAndGlobalRateLimitPolicies(
        this IServiceCollection services,
        Assembly controllersAssembly
    )
    {
        var userLimits = new HashSet<(int, int)>();
        var globalLimits = new HashSet<(int, int)>();

        foreach (
            var type in controllersAssembly
                .GetTypes()
                .Where(t =>
                    t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t)
                )
        )
        {
            foreach (var attr in type.GetCustomAttributes<UserRateLimitAttribute>(true))
            {
                userLimits.Add((attr.Count, attr.Seconds));
            }

            foreach (var attr in type.GetCustomAttributes<GlobalRateLimitAttribute>(true))
            {
                globalLimits.Add((attr.Count, attr.Seconds));
            }

            foreach (
                var method in type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
                )
            )
            {
                foreach (var attr in method.GetCustomAttributes<UserRateLimitAttribute>(true))
                {
                    userLimits.Add((attr.Count, attr.Seconds));
                }

                foreach (var attr in method.GetCustomAttributes<GlobalRateLimitAttribute>(true))
                {
                    globalLimits.Add((attr.Count, attr.Seconds));
                }
            }
        }

        services.AddRateLimiter(options =>
        {
            foreach (var (count, seconds) in userLimits)
            {
                AddUserRateLimitPolicy(options, count, seconds);
            }

            foreach (var (count, seconds) in globalLimits)
            {
                AddGlobalRateLimitPolicy(options, count, seconds);
            }
        });

        return services;
    }

    public static void AddUserRateLimitPolicy(RateLimiterOptions options, int count, int seconds)
    {
        string policyName = $"User_{count}:{seconds}";

        options.AddPolicy(
            policyName,
            context =>
            {
                string userId =
                    context.User.FindFirst("sub")?.Value ?? context
                        .User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                        ?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = count,
                        Window = TimeSpan.FromSeconds(seconds),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }
                );
            }
        );
    }

    public static void AddGlobalRateLimitPolicy(RateLimiterOptions options, int count, int seconds)
    {
        string policyName = $"Global_{count}:{seconds}";

        options.AddPolicy(
            policyName,
            context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: "global",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = count,
                        Window = TimeSpan.FromSeconds(seconds),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }
                )
        );
    }
}
