using Algowars.Application.Queries.Permissions.GetUserHasAccess;
using MediatR;

namespace Algowars.Application.Services.Authorization;

public interface IPermissionAccessService
{
    Task<bool> HasAccessAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
}

public sealed class PermissionAccessService(IMediator mediator) : IPermissionAccessService
{
    public async Task<bool> HasAccessAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetUserHasAccessQuery(userId, permissionCode), cancellationToken);

        return result.IsSuccess && result.Value;
    }
}