using Algowars.Api.Attributes;
using Algowars.Api.Context;
using Algowars.Application.Dtos.Users;
using Algowars.Application.Queries.Users.GetUserBySub;
using MediatR;
using System.Security.Claims;

namespace Algowars.Api.Middleware;

internal sealed partial class UserContextMiddleware(
    ISender sender,
    IUserContext userContext,
    ILogger<UserContextMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequiresUserAttribute>() is null)
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

        var result = await sender.Send(new GetUserBySubQuery(sub), context.RequestAborted);
        if (result.IsSuccess)
            userContext.User = result.Value;
        else
            LogResolveFailed(sub, context.Request.Path);

        await next(context);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "User context: missing sub claim on [RequiresUser] endpoint {Path}")]
    private partial void LogMissingSub(string path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "User context: failed to resolve user for sub {Sub} on {Path}")]
    private partial void LogResolveFailed(string sub, string path);
}
