using Algowars.Domain.Authorization;
using Ardalis.Result;

namespace Algowars.Application.Queries.Permissions.GetUserHasAccess;

internal sealed class GetUserHasAccessHandler(IAuthorizationReadRepository authorizationRepository) : IQueryHandler<GetUserHasAccessQuery, bool>
{
    public async Task<Result<bool>> Handle(GetUserHasAccessQuery request, CancellationToken cancellationToken)
    {
        bool? hasAccess = await authorizationRepository.UserHasAccessAsync(
            request.UserId,
            request.PermissionCode,
            cancellationToken);

        if (hasAccess is null)
            return Result.NotFound();

        return Result.Success(hasAccess.Value);
    }
}