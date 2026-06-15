using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Algowars.ServiceDefaults;

public static class Extensions
{
    // Service defaults configuration added in task 03
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder) => builder;

    public static WebApplication MapDefaultEndpoints(this WebApplication app) => app;
}
