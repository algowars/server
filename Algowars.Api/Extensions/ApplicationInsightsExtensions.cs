namespace Algowars.Api.Extensions;

public static class ApplicationInsightsExtensions
{
    public static IServiceCollection AddAppInsights(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration);
        return services;
    }
}
