using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Accounts.UpdateUsername;

public sealed class UpdateUsernameHandler(
    IAccountRepository accounts,
    IValidator<UpdateUsernameCommand> validator
) : AbstractCommandHandler<UpdateUsernameCommand, UpdateUsernameResult>(validator)
{
    protected override async Task<Result<UpdateUsernameResult>> HandleValidated(
        UpdateUsernameCommand request,
        CancellationToken cancellationToken
    )
    {
        AccountModel? account = await accounts.GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.NotFound();
        }

        bool usernameTaken = await accounts.ExistsByUsernameAsync(request.NewUsername, cancellationToken);

        if (usernameTaken)
        {
            return Result.Invalid(new ValidationError("Username", "Username is already taken."));
        }

        account.ChangeUsername(request.NewUsername);

        await accounts.UpdateUsernameAsync(account.Id, account.Username, account.UsernameLastChangedAt!.Value, cancellationToken);

        return Result.Success(new UpdateUsernameResult(
            account.Id,
            account.Username,
            account.UsernameLastChangedAt!.Value
        ));
    }
}