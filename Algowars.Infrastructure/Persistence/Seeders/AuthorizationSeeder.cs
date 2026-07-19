using Algowars.Domain.Authorization.Rbac;
using Algowars.Domain.Authorization.Rbac.Entities;
using Algowars.Domain.Authorization.Rbac.Enums;
using Algowars.Domain.Authorization.Rbac.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders;

internal sealed class AuthorizationSeeder(AlgowarsDbContext context) : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        PermissionCode createSubmissionCode = new(WellKnownAuthorization.CreateSubmissionPermission);

        Permission permission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Code == createSubmissionCode, cancellationToken)
            ?? Permission.Create(createSubmissionCode, "Allows creating code submissions");

        if (context.Entry(permission).State == EntityState.Detached)
            await context.Permissions.AddAsync(permission, cancellationToken);

        Role role = await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == new Name(WellKnownAuthorization.DefaultUserRole), cancellationToken)
            ?? Role.Create(new Name(WellKnownAuthorization.DefaultUserRole));

        if (context.Entry(role).State == EntityState.Detached)
            await context.Roles.AddAsync(role, cancellationToken);

        Group group = await context.Groups
            .Include(g => g.RoleGrants)
            .FirstOrDefaultAsync(g => g.Name == new Name(WellKnownAuthorization.DefaultUserGroup), cancellationToken)
            ?? Group.Create(new Name(WellKnownAuthorization.DefaultUserGroup));

        if (context.Entry(group).State == EntityState.Detached)
            await context.Groups.AddAsync(group, cancellationToken);

        role.GrantPermission(permission.Id);
        group.GrantRole(role);

        await context.SaveChangesAsync(cancellationToken);
    }
}
