using Algowars.Domain.Authorization;
using Algowars.Domain.Authorization.Rbac.Enums;
using Algowars.Domain.Authorization.Rbac.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal class AuthorizationReadRepository(AlgowarsDbContext context) : IAuthorizationReadRepository
{
    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId uid = new(userId);

        List<(string Code, DecisionEffect Effect)> restrictionRows = await context.SecurityRestrictions
            .Where(r => r.UserId == uid && r.ExpiresAt >= now)
            .Select(r => new ValueTuple<string, DecisionEffect>(r.PermissionCode.Value, r.Effect))
            .ToListAsync(cancellationToken);

        List<(string Code, DecisionEffect Effect)> roleRows = await (
            from userGroup in context.Set<Dictionary<string, object>>("user_groups")
            join groupRole in context.Set<Dictionary<string, object>>("group_roles")
                on EF.Property<GroupId>(userGroup, "group_id") equals EF.Property<GroupId>(groupRole, "group_id")
            join rolePermission in context.RolePermissions
                on EF.Property<RoleId>(groupRole, "role_id") equals EF.Property<RoleId>(rolePermission, "role_id")
            join permission in context.Permissions
                on rolePermission.PermissionId equals permission.Id
            where EF.Property<Guid>(userGroup, "user_id") == userId
            select new ValueTuple<string, DecisionEffect>(permission.Code.Value, rolePermission.Effect))
            .ToListAsync(cancellationToken);

        Dictionary<string, HashSet<DecisionEffect>> restrictionsByCode = restrictionRows
            .GroupBy(r => r.Code)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Effect).ToHashSet());

        Dictionary<string, HashSet<DecisionEffect>> roleEffectsByCode = roleRows
            .GroupBy(r => r.Code)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Effect).ToHashSet());

        IEnumerable<string> allCodes = restrictionsByCode.Keys.Union(roleEffectsByCode.Keys);

        return allCodes
            .Where(code => HasAccess(code, restrictionsByCode, roleEffectsByCode))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await (
            from userGroup in context.Set<Dictionary<string, object>>("user_groups")
            join groupRole in context.Set<Dictionary<string, object>>("group_roles")
                on EF.Property<GroupId>(userGroup, "group_id") equals EF.Property<GroupId>(groupRole, "group_id")
            join role in context.Roles
                on EF.Property<RoleId>(groupRole, "role_id") equals role.Id
            where EF.Property<Guid>(userGroup, "user_id") == userId
            select role.Name.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

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

    // Same precedence as UserHasAccessAsync, applied per permission code:
    // an active restriction on this code wins outright (Deny beats Allow);
    // with no restriction, fall back to role-derived effects (Deny still beats Allow).
    private static bool HasAccess(
        string code,
        Dictionary<string, HashSet<DecisionEffect>> restrictionsByCode,
        Dictionary<string, HashSet<DecisionEffect>> roleEffectsByCode)
    {
        if (restrictionsByCode.TryGetValue(code, out var restrictionEffects))
        {
            if (restrictionEffects.Contains(DecisionEffect.Deny))
                return false;

            if (restrictionEffects.Contains(DecisionEffect.Allow))
                return true;
        }

        if (roleEffectsByCode.TryGetValue(code, out var roleEffects))
        {
            if (roleEffects.Contains(DecisionEffect.Deny))
                return false;

            return roleEffects.Contains(DecisionEffect.Allow);
        }

        return false;
    }
}