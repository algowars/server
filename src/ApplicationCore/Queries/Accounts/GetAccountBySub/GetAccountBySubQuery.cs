using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Queries.Accounts.GetAccountBySub;

public sealed record GetAccountBySubQuery(string Sub) : IQuery<AccountDto>;