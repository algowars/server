using Algowars.Domain.Authorization;
using Algowars.Domain.Authorization.Rbac.Enums;
using Algowars.Domain.Authorization.Rbac.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal class AuthorizationReadRepository(AlgowarsDbContext context) : IAuthorizationReadRepository
{
    public async Task<bool?> UserHasAccessAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        PermissionCode code = new(permissionCode);
        UserId uid = new(userId);

        List<DecisionEffect> restrictionEffects = await context.SecurityRestrictions
            .Where(r => r.UserId == uid && r.PermissionCode == code && r.ExpiresAt >= now)
            .Select(r => r.Effect)
            .ToListAsync(cancellationToken);

        if (restrictionEffects.Contains(DecisionEffect.Deny))
            return false;

        if (restrictionEffects.Contains(DecisionEffect.Allow))
            return true;

        List<DecisionEffect> roleEffects = await (
            from userGroup in context.Set<Dictionary<string, object>>("user_groups")
            join groupRole in context.Set<Dictionary<string, object>>("group_roles")
                on EF.Property<GroupId>(userGroup, "group_id") equals EF.Property<GroupId>(groupRole, "group_id")
            join rolePermission in context.RolePermissions
                on EF.Property<RoleId>(groupRole, "role_id") equals EF.Property<RoleId>(rolePermission, "role_id")
            join permission in context.Permissions
                on rolePermission.PermissionId equals permission.Id
            where EF.Property<Guid>(userGroup, "user_id") == userId
                  && permission.Code == code
            select rolePermission.Effect)
            .ToListAsync(cancellationToken);

        if (roleEffects.Contains(DecisionEffect.Deny))
            return false;

        return roleEffects.Contains(DecisionEffect.Allow);
    }
}