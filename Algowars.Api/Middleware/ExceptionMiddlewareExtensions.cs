using System.Text.Json;

namespace Algowars.Api.Middleware;

internal static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var ex = feature?.Error ?? new Exception("Unknown error");

                logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { message = "An unexpected error occurred." }));
            });
        });

        return app;
    }

    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
        => app.UseMiddleware<UserContextMiddleware>();
}
