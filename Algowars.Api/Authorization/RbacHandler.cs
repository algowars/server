using Microsoft.AspNetCore.Authorization;

namespace Algowars.Api.Authorization;

public sealed class RbacHandler : AuthorizationHandler<RbacRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RbacRequirement requirement)
    {
        var permission = context.User.FindFirst(c => c.Type == "permissions" && c.Value == requirement.Permission);
        if (permission is not null)
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
