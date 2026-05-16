using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Account;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext db) : IAccountRepository
{
    public async Task AddAsync(AccountModel account, CancellationToken ct)
    {
        var entity = account.Adapt<AccountEntity>();

        db.Accounts.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<AccountModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db
            .Accounts.ProjectToType<AccountModel>()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AccountModel?> GetByUsernameOrSubAsync(
        string username,
        string sub,
        CancellationToken cancellationToken
    )
    {
        return await db
            .Accounts.ProjectToType<AccountModel>()
            .SingleOrDefaultAsync(a => a.Username == username || a.Sub == sub, cancellationToken);
    }

    public async Task<AccountModel?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken
    )
    {
        return await db
            .Accounts.ProjectToType<AccountModel>()
            .SingleOrDefaultAsync(a => a.Username == username, cancellationToken);
    }

    public async Task<AccountModel?> GetBySubAsync(string sub, CancellationToken cancellationToken)
    {
        return await db
            .Accounts
            .Where(a => a.Sub == sub)
            .ProjectToType<AccountModel>()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db
            .Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            is not null;
    }

    public async Task UpdateImageUrlAsync(Guid id, string? imageUrl, CancellationToken cancellationToken)
    {
        await db.Accounts
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.ImageUrl, imageUrl),
                cancellationToken
            );
    }

    public async Task<int> CountByUsernameBaseAsync(string usernameBase, CancellationToken cancellationToken)
    {
        return await db.Accounts
            .Where(a => a.Username == usernameBase || a.Username.StartsWith(usernameBase + "_"))
            .CountAsync(cancellationToken);
    }

    public async Task UpdateUsernameAsync(Guid id, string username, DateTime usernameLastChangedAt, CancellationToken cancellationToken)
    {
        await db.Accounts
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(a => a.Username, username)
                    .SetProperty(a => a.UsernameLastChangedAt, usernameLastChangedAt),
                cancellationToken
            );
    }

    public async Task UpdateAboutAsync(Guid id, string? about, CancellationToken cancellationToken)
    {
        await db.Accounts
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.About, about),
                cancellationToken
            );
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await db.Accounts
            .AnyAsync(a => a.Username == username, cancellationToken);
    }
}