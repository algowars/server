using Algowars.Api.Attributes;
using Algowars.Application;
using Algowars.Application.Services.Users;
using Microsoft.ApplicationInsights.DataContracts;
using System.Security.Claims;

namespace Algowars.Api.Middleware;

public partial class AccountContextMiddleware(
    IUserService userService,
    UserContext userContext,
    ILogger<AccountContextMiddleware> logger
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequireUserAttribute>() == null)
        {
            await next(context);
            return;
        }

        string? sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub))
        {
            LogMissingSub(context.Request.Path);
            await next(context);
            return;
        }

        var result = await userService.GetBySubAsync(sub, context.RequestAborted);
        if (result.IsSuccess)
        {
            userContext.User = result.Value;
            userContext.Permissions = [.. context.User
                .FindAll("permissions")
                .Select(c => c.Value)];

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            if (requestTelemetry is not null)
            {
                requestTelemetry.Properties.TryAdd("account.id", userContext.User.Id.ToString());
                requestTelemetry.Properties.TryAdd("account.username", userContext.User.Username);
            }
        }
        else
        {
            LogResolveFailed(sub, context.Request.Path, string.Join(", ", result.Errors));
        }

        await next(context);
    }

    [LoggerMessage(
        EventId = LoggingEventIds.Accounts.ContextMissingSub,
        Level = LogLevel.Warning,
        Message = "Account context: missing sub claim on [RequiresAccount] endpoint {path}")]
    private partial void LogMissingSub(string path);

    [LoggerMessage(
        EventId = LoggingEventIds.Accounts.ContextResolveFailed,
        Level = LogLevel.Warning,
        Message = "Account context: failed to resolve account for sub {sub} on {path}: {errors}")]
    private partial void LogResolveFailed(string sub, string path, string errors);
}