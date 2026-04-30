using PublicApi.Middleware;

namespace PublicApi.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAccountContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AccountContextMiddleware>();
    }
}