using Algowars.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Settings;

public static class OptionExtensions
{
    public static IServiceCollection AddOption<T>(
        this IServiceCollection services,
        IConfiguration configuration)
        where T : class, IOption
    {
        var instance = configuration.GetSection(T.SectionName).Get<T>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{T.SectionName}' is missing or invalid.");

        services.AddSingleton(instance);
        return services;
    }
}
