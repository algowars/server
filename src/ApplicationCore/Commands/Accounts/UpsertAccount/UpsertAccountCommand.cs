namespace ApplicationCore.Commands.Accounts.UpsertAccount;

public sealed record UpsertAccountCommand(string Sub, string? ImageUrl) : ICommand<AccountUpsertResult>;

public sealed record AccountUpsertResult(Guid Id, string Username, string? ImageUrl, DateTime CreatedOn);