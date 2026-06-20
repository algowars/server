using Algowars.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Algowars.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuth0(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var auth0Options = configuration.GetSection(Auth0Options.SectionName).Get<Auth0Options>()
            ?? throw new InvalidOperationException($"Configuration section '{Auth0Options.SectionName}' is missing or invalid.");

        services.AddSingleton(auth0Options);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{auth0Options.Domain}/";
                options.Audience = auth0Options.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                };
            });

        return services;
    }
}
