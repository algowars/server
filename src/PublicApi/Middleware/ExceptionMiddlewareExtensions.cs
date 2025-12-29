using System.Net;
using System.Text.Json;
using ApplicationCore.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PublicApi.Middleware;

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
                var logger = context
                    .RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var ex = feature?.Error;

                context.Response.ContentType = "application/json";

                if (ex is FluentValidation.ValidationException validationException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    var errors = validationException.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                    });

                    AppLog.UnhandledExceptionWithPath(
                        logger,
                        context.Request.Method,
                        context.Request.Path,
                        ex
                    );

                    var problem = new
                    {
                        status = context.Response.StatusCode,
                        title = "Validation Failed",
                        traceId = context.TraceIdentifier,
                        path = context.Request.Path.Value,
                        errors,
                    };

                    string json = JsonSerializer.Serialize(problem);
                    await context.Response.WriteAsync(json);
                    return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                if (ex is not null)
                {
                    AppLog.UnhandledExceptionWithPath(
                        logger,
                        context.Request.Method,
                        context.Request.Path,
                        ex
                    );
                }
                else
                {
                    AppLog.UnhandledException(logger, new Exception("Unknown error"));
                }

                var genericProblem = new
                {
                    status = context.Response.StatusCode,
                    title = "An unexpected error occurred.",
                    traceId = context.TraceIdentifier,
                    path = context.Request.Path.Value,
                    details = env.IsDevelopment() ? ex?.ToString() : null,
                };

                string genericJson = JsonSerializer.Serialize(genericProblem);
                await context.Response.WriteAsync(genericJson);
            });
        });

        return app;
    }
}

public static partial class AppLog
{
    [LoggerMessage(
        EventId = LoggingEventIds.Exceptions.UnhandledException,
        Level = LogLevel.Error,
        Message = "Unhandled exception."
    )]
    public static partial void UnhandledException(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = LoggingEventIds.Exceptions.UnhandledExceptionWithPath,
        Level = LogLevel.Error,
        Message = "Unhandled exception for {Method} {Path}."
    )]
    public static partial void UnhandledExceptionWithPath(
        ILogger logger,
        string method,
        string path,
        Exception exception
    );
}
