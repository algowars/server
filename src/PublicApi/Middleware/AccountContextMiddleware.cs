using PublicApi.Attributes;

namespace PublicApi.Middleware;

using System.Security.Claims;
using System.Threading.Tasks;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Http;

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

        string? sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
        if (!string.IsNullOrEmpty(sub))
        {
            string? imageUrl = context.User.FindFirst("picture")?.Value;

            var result = await accountAppService.UpsertAccountAsync(sub, imageUrl, context.RequestAborted);
            if (result.IsSuccess)
            {
                accountContext.Account = new ApplicationCore.Dtos.Accounts.AccountDto
                {
                    Id = result.Value.Id,
                    Username = result.Value.Username,
                    ImageUrl = result.Value.ImageUrl,
                    CreatedOn = result.Value.CreatedOn,
                };
            }
        }

        await next(context);
    }
}
