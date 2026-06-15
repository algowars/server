using Algowars.Api.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Threading.RateLimiting;

namespace Algowars.Api.Extensions;

internal static class RateLimitExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, Assembly controllersAssembly)
    {
        var userLimits = new HashSet<(int, int)>();
        var globalLimits = new HashSet<(int, int)>();

        foreach (var type in controllersAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            foreach (var attr in type.GetCustomAttributes<UserRateLimitAttribute>(true))
                userLimits.Add((attr.Count, attr.Seconds));
            foreach (var attr in type.GetCustomAttributes<GlobalRateLimitAttribute>(true))
                globalLimits.Add((attr.Count, attr.Seconds));

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                foreach (var attr in method.GetCustomAttributes<UserRateLimitAttribute>(true))
                    userLimits.Add((attr.Count, attr.Seconds));
                foreach (var attr in method.GetCustomAttributes<GlobalRateLimitAttribute>(true))
                    globalLimits.Add((attr.Count, attr.Seconds));
            }
        }

        services.AddRateLimiter(options =>
        {
            foreach (var (count, seconds) in userLimits)
                AddUserPolicy(options, count, seconds);
            foreach (var (count, seconds) in globalLimits)
                AddGlobalPolicy(options, count, seconds);
        });

        return services;
    }

    private static void AddUserPolicy(RateLimiterOptions options, int count, int seconds)
    {
        options.AddPolicy($"User_{count}:{seconds}", context =>
        {
            string userId = context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = count,
                Window = TimeSpan.FromSeconds(seconds),
                QueueLimit = 0,
            });
        });
    }

    private static void AddGlobalPolicy(RateLimiterOptions options, int count, int seconds)
    {
        options.AddPolicy($"Global_{count}:{seconds}", _ =>
            RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = count,
                Window = TimeSpan.FromSeconds(seconds),
                QueueLimit = 0,
            }));
    }
}
