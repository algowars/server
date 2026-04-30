using PublicApi.Attributes;

namespace PublicApi.Middleware;

using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

public class AccountContextMiddleware(
    IAccountAppService accountAppService,
    IAccountContext accountContext
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequiresAccountAttribute>() == null)
        {
            await next(context);
            return;
        }

        string? sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(sub))
        {
            var result = await accountAppService.GetAccountBySubAsync(sub, context.RequestAborted);
            if (result.IsSuccess)
            {
                accountContext.Account = result.Value;
            }
        }

        await next(context);
    }
}