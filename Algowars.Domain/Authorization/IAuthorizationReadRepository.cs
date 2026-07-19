namespace Algowars.Domain.Authorization;

public interface IAuthorizationReadRepository
{
    Task<bool?> UserHasAccessAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
}