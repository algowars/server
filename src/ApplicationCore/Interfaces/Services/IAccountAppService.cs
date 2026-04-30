using ApplicationCore.Commands.Accounts.UpdateUsername;
using ApplicationCore.Commands.Accounts.UpsertAccount;
using ApplicationCore.Dtos.Accounts;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface IAccountAppService
{
    Task<Result<Guid>> CreateAsync(
        string username,
        string sub,
        string imageUrl,
        CancellationToken cancellationToken
    );

    Task<Result<AccountDto>> GetAccountBySubAsync(string sub, CancellationToken cancellationToken);

    Task<Result<ProfileAggregateDto>> GetProfileAggregateAsync(
        string username,
        CancellationToken cancellationToken
    );

    Task<ProfileSettingsDto?> GetProfileSettingsAsync(
        string sub,
        CancellationToken cancellationToken
    );

    Task<Result<AccountUpsertResult>> UpsertAccountAsync(
        string sub,
        string? imageUrl,
        CancellationToken cancellationToken
    );

    Task<Result<UpdateUsernameResult>> UpdateUsernameAsync(
        Guid accountId,
        string newUsername,
        DateTime? usernameLastChangedAt,
        CancellationToken cancellationToken
    );
}