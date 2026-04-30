using Microsoft.AspNetCore.Authorization;
using PublicApi.Authorization;

namespace PublicApi.Extensions;

public static class AuthorizationExtensions
{
    private static readonly string[] permissions =
    [
        "create:problems",
        "read:admin-problems",
        "read:admin-problem",
        "read:languages",
    ];

    public static IServiceCollection AddRbacAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (string? permission in permissions)
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
}