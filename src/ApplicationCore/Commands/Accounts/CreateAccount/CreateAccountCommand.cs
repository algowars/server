namespace ApplicationCore.Commands.Accounts.CreateAccount;

public sealed record CreateAccountCommand(string Username, string Sub, string ImageUrl)
    : ICommand<Guid>;