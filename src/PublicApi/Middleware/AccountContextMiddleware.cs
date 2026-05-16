using ApplicationCore.Logging;
using Microsoft.ApplicationInsights.DataContracts;
using PublicApi.Attributes;

namespace PublicApi.Middleware;

using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

public partial class AccountContextMiddleware(
    IAccountAppService accountAppService,
    IAccountContext accountContext,
    ILogger<AccountContextMiddleware> logger
) : IMiddleware
{
    private readonly ILogger<AccountContextMiddleware> _logger = logger;
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequiresAccountAttribute>() == null)
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

        var result = await accountAppService.GetAccountBySubAsync(sub, context.RequestAborted);
        if (result.IsSuccess)
        {
            accountContext.Account = result.Value;

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            if (requestTelemetry is not null && accountContext.Account is not null)
            {
                var account = accountContext.Account;
                requestTelemetry.Properties.TryAdd("account.id", account.Id?.ToString() ?? string.Empty);
                requestTelemetry.Properties.TryAdd("account.username", account.Username);
                requestTelemetry.Properties.TryAdd("account.permissions", string.Join(",", account.Permissions));
                requestTelemetry.Properties.TryAdd("account.createdOn", account.CreatedOn.ToString("O"));
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