using ApplicationCore.Domain.Accounts;

namespace ApplicationCore.Interfaces.Repositories;

public interface IAccountRepository
{
    Task AddAsync(AccountModel accountModel, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<AccountModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccountModel?> GetBySubAsync(string sub, CancellationToken cancellationToken);

    Task<AccountModel?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    Task<AccountModel?> GetByUsernameOrSubAsync(
        string username,
        string sub,
        CancellationToken cancellationToken
    );
}