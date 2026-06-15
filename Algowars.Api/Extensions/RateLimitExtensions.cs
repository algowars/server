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
            CollectFromMember(type, userLimits, globalLimits);

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                CollectFromMember(method, userLimits, globalLimits);
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

    private static void CollectFromMember(MemberInfo member, HashSet<(int, int)> userLimits, HashSet<(int, int)> globalLimits)
    {
        foreach (var attr in member.GetCustomAttributes<UserRateLimitAttribute>(true))
            userLimits.Add((attr.Count, attr.Seconds));

        foreach (var attr in member.GetCustomAttributes<GlobalRateLimitAttribute>(true))
            globalLimits.Add((attr.Count, attr.Seconds));

        foreach (var attr in member.GetCustomAttributes<EnableRateLimitingAttribute>(true))
            if (attr.PolicyName is not null)
                ParsePolicyName(attr.PolicyName, userLimits, globalLimits);
    }

    private static void ParsePolicyName(string policyName, HashSet<(int, int)> userLimits, HashSet<(int, int)> globalLimits)
    {
        bool isUser = policyName.StartsWith("User_", StringComparison.Ordinal);
        bool isGlobal = policyName.StartsWith("Global_", StringComparison.Ordinal);

        if (!isUser && !isGlobal)
            return;

        string suffix = isUser
            ? policyName["User_".Length..]
            : policyName["Global_".Length..];

        string[] parts = suffix.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out int count) && int.TryParse(parts[1], out int seconds))
        {
            if (isUser) userLimits.Add((count, seconds));
            else globalLimits.Add((count, seconds));
        }
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
