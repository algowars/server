using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Accounts.UpdateProfileSettings;

public sealed class UpdateProfileSettingsHandler(
    IAccountRepository accounts,
    IValidator<UpdateProfileSettingsCommand> validator
) : AbstractCommandHandler<UpdateProfileSettingsCommand, UpdateProfileSettingsResult>(validator)
{
    protected override async Task<Result<UpdateProfileSettingsResult>> HandleValidated(
        UpdateProfileSettingsCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await accounts.GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.NotFound();
        }

        await accounts.UpdateAboutAsync(account.Id, request.Bio, cancellationToken);

        return Result.Success(new UpdateProfileSettingsResult(request.Bio));
    }
}
