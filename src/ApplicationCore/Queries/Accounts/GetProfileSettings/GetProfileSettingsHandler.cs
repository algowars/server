using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Accounts.GetProfileSettings;

public sealed class GetProfileSettingsHandler(IAccountRepository accountRepository)
    : IQueryHandler<GetProfileSettingsQuery, ProfileSettingsDto>
{
    public async Task<Result<ProfileSettingsDto>> Handle(
        GetProfileSettingsQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var account = await accountRepository.GetBySubAsync(request.Sub, cancellationToken);

            return account is null
                ? Result.NotFound()
                : Result.Success(new ProfileSettingsDto(account.Username));
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
