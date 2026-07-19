using Algowars.Domain.Authorization.Rbac.Entities;
using Algowars.Domain.Authorization.Rbac.ValueObjects;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class UserWriteRepository(AlgowarsDbContext context) : IUserWriteRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToGroupAsync(Guid userId, string groupName, CancellationToken cancellationToken = default)
    {
        Group? group = await context.Groups.FirstOrDefaultAsync(
            g => g.Name == new Name(groupName),
            cancellationToken);

        if (group is null)
            return;

        var joinEntity = context.Set<Dictionary<string, object>>("user_groups");

        bool alreadyAssigned = await joinEntity.AnyAsync(
            ug => EF.Property<Guid>(ug, "user_id") == userId
               && EF.Property<GroupId>(ug, "group_id") == group.Id,
            cancellationToken);

        if (alreadyAssigned)
            return;

        joinEntity.Add(new Dictionary<string, object>
        {
            ["user_id"] = userId,
            ["group_id"] = group.Id
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Sub == sub, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> FindByUsername(Username username, CancellationToken cancellationToken)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }
}
