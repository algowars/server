using ApplicationCore.Settings;

namespace PublicApi.Extensions;

public static class SettingsRegistrationExtensions
{
    public static void RegisterAppSettings(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        RegisterSetting<ConnectionStringsSettings>(services, configuration);
        RegisterSetting<MediatRSettings>(services, configuration);
    }

    private static void RegisterSetting<T>(
        IServiceCollection services,
        IConfiguration configuration
    )
        where T : class, ISettings
    {
        services.Configure<T>(configuration.GetSection(T.SectionKey));
    }
}