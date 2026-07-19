using Algowars.Application;
using Algowars.Application.Services.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Algowars.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission) : base(typeof(RequirePermissionFilter))
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission is required.", nameof(permission));

        Arguments = [permission];
    }
}

public sealed class RequirePermissionFilter(
    IPermissionAccessService permissionAccessService,
    UserContext userContext,
    string permission) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (userContext.User is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        bool hasAccess = await permissionAccessService.HasAccessAsync(
            userContext.User.Id,
            permission,
            context.HttpContext.RequestAborted);

        if (!hasAccess)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}