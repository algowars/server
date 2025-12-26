namespace ApplicationCore.Commands.Account.CreateAccount;

public sealed record CreateAccountCommand(string Username, string Sub, string ImageUrl)
    : ICommand<Guid>;
