using Asp.Versioning;
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

        return services;
    }
}
