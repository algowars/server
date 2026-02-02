using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Accounts.GetAccountBySub;

public sealed class GetAccountBySubHandler(IAccountRepository repository)
    : IQueryHandler<GetAccountBySubQuery, AccountDto>
{
    public async Task<Result<AccountDto>> Handle(GetAccountBySubQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Sub))
        {
            return Result.Invalid([new ValidationError(nameof(request.Sub), "Sub is required")]);
        }

        try
        {
            var account = await repository.GetBySubAsync(request.Sub, ct);

            if (account is null)
            {
                return Result.NotFound();
            }

            var dto = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                ImageUrl = account.ImageUrl,
                CreatedOn = account.CreatedOn,
            };
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}