using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Account;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class AccountRepository(IAppDbContext db) : IAccountRepository
{
    public async Task AddAsync(AccountModel account, CancellationToken ct)
    {
        var entity = account.Adapt<AccountEntity>();

        db.Accounts.Add(entity);
        await db.SaveChangesAsync(ct);

        account.Id = entity.Id;
    }

    public async Task<AccountModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db
            .Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        return entity?.Adapt<AccountModel>();
    }

    public async Task<AccountModel?> GetByUsernameOrSubAsync(
        string username,
        string sub,
        CancellationToken cancellationToken
    )
    {
        var entity = await db
            .Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Username == username || a.Sub == sub, cancellationToken);

        return entity?.Adapt<AccountModel>();
    }

    public async Task<AccountModel?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken
    )
    {
        var entity = await db
            .Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Username == username, cancellationToken);

        return entity?.Adapt<AccountModel>();
    }

    public async Task<AccountModel?> GetBySubAsync(string sub, CancellationToken cancellationToken)
    {
        var entity = await db
            .Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Sub == sub, cancellationToken);

        return entity?.Adapt<AccountModel>();
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db
            .Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            is not null;
    }
}
