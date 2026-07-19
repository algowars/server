using Algowars.Domain.Authorization.Rbac;
using Algowars.Domain.Authorization.Rbac.Entities;
using Algowars.Domain.Authorization.Rbac.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders;

internal sealed class AuthorizationSeeder(AlgowarsDbContext context) : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        PermissionCode createSubmissionCode = new(WellKnownAuthorization.CreateSubmissionPermission);

        Permission createSubmissionPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Code == createSubmissionCode, cancellationToken)
            ?? Permission.Create(createSubmissionCode, "Allows creating code submissions");

        if (context.Entry(createSubmissionPermission).State == EntityState.Detached)
            await context.Permissions.AddAsync(createSubmissionPermission, cancellationToken);

        PermissionCode readAdminProblemsCode = new(WellKnownAuthorization.ReadAdminProblemsPermission);

        Permission readAdminProblemsPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Code == readAdminProblemsCode, cancellationToken)
            ?? Permission.Create(readAdminProblemsCode, "Allows viewing admin-only problem data");

        if (context.Entry(readAdminProblemsPermission).State == EntityState.Detached)
            await context.Permissions.AddAsync(readAdminProblemsPermission, cancellationToken);

        PermissionCode readAdminUsersCode = new(WellKnownAuthorization.ReadAdminUsersPermission);

        Permission readAdminUsersPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Code == readAdminUsersCode, cancellationToken)
            ?? Permission.Create(readAdminUsersCode, "Allows viewing users in the admin panel");

        if (context.Entry(readAdminUsersPermission).State == EntityState.Detached)
            await context.Permissions.AddAsync(readAdminUsersPermission, cancellationToken);

        Role role = await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == new Name(WellKnownAuthorization.DefaultUserRole), cancellationToken)
            ?? Role.Create(new Name(WellKnownAuthorization.DefaultUserRole));

        if (context.Entry(role).State == EntityState.Detached)
            await context.Roles.AddAsync(role, cancellationToken);

        Role adminRole = await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == new Name(WellKnownAuthorization.AdminRole), cancellationToken)
            ?? Role.Create(new Name(WellKnownAuthorization.AdminRole));

        if (context.Entry(adminRole).State == EntityState.Detached)
            await context.Roles.AddAsync(adminRole, cancellationToken);

        Group group = await context.Groups
            .Include(g => g.RoleGrants)
            .FirstOrDefaultAsync(g => g.Name == new Name(WellKnownAuthorization.DefaultUserGroup), cancellationToken)
            ?? Group.Create(new Name(WellKnownAuthorization.DefaultUserGroup));

        if (context.Entry(group).State == EntityState.Detached)
            await context.Groups.AddAsync(group, cancellationToken);

        Group adminGroup = await context.Groups
            .Include(g => g.RoleGrants)
            .FirstOrDefaultAsync(g => g.Name == new Name(WellKnownAuthorization.AdminGroup), cancellationToken)
            ?? Group.Create(new Name(WellKnownAuthorization.AdminGroup));

        if (context.Entry(adminGroup).State == EntityState.Detached)
            await context.Groups.AddAsync(adminGroup, cancellationToken);

        role.GrantPermission(createSubmissionPermission.Id);

        adminRole.GrantPermission(readAdminProblemsPermission.Id);
        adminRole.GrantPermission(readAdminUsersPermission.Id);

        group.GrantRole(role);
        adminGroup.GrantRole(adminRole);

        await context.SaveChangesAsync(cancellationToken);
    }
}