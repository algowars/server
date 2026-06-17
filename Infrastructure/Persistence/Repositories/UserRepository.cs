using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AlgowarsDbContext context) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
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

    public async Task<User?> FindByUsername(Username username, CancellationToken cancellationToken)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }
}
