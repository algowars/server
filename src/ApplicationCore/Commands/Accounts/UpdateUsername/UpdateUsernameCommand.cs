namespace ApplicationCore.Commands.Accounts.UpdateUsername;

public sealed record UpdateUsernameCommand(Guid AccountId, string NewUsername, DateTime? UsernameLastChangedAt) : ICommand<UpdateUsernameResult>;

public sealed record UpdateUsernameResult(Guid Id, string Username, DateTime UsernameLastChangedAt);