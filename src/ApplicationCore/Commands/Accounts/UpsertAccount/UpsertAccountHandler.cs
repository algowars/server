using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using Bogus;
using FluentValidation;

namespace ApplicationCore.Commands.Accounts.UpsertAccount;

public sealed class UpsertAccountHandler(
    IAccountRepository accounts,
    IValidator<UpsertAccountCommand> validator
) : AbstractCommandHandler<UpsertAccountCommand, AccountUpsertResult>(validator)
{
    private static readonly Faker Faker = new();

    protected override async Task<Result<AccountUpsertResult>> HandleValidated(
        UpsertAccountCommand request,
        CancellationToken cancellationToken
    )
    {
          AccountModel? existing = await accounts.GetBySubAsync(request.Sub, cancellationToken);

        if (existing is not null)
        {
            await accounts.UpdateImageUrlAsync(existing.Id, request.ImageUrl, cancellationToken);

            return Result.Success(new AccountUpsertResult(
                existing.Id,
                existing.Username,
                request.ImageUrl,
                existing.CreatedOn
            ));
        }

        Guid id = Guid.NewGuid();
        string username = await GenerateUniqueUsernameAsync(accounts, cancellationToken);

        AccountModel account = new()
        {
            Id = id,
            Username = username,
            Sub = request.Sub,
            ImageUrl = request.ImageUrl,
            LastModifiedById = null,
        };

        await accounts.AddAsync(account, cancellationToken);

        return Result.Success(new AccountUpsertResult(id, username, request.ImageUrl, DateTime.UtcNow));
    }

    private static async Task<string> GenerateUniqueUsernameAsync(
        IAccountRepository accounts,
        CancellationToken cancellationToken
    )
    {
        string usernameBase = $"{Faker.Hacker.Adjective()}_{Faker.Hacker.Noun()}"
            .ToLowerInvariant()
            .Replace(" ", "_");

        int count = await accounts.CountByUsernameBaseAsync(usernameBase, cancellationToken);

        return count == 0 ? usernameBase : $"{usernameBase}_{count}";
    }
}
