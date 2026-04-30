using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Domain.Accounts;

public interface IAccountContext
{
    AccountDto? Account { get; set; }
}

public sealed class AccountContext : IAccountContext
{
    public AccountDto? Account { get; set; }
}