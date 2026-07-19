namespace Algowars.Domain.Authorization;

public interface IAuthorizationReadRepository
{
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool?> UserHasAccessAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
}