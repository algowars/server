namespace ApplicationCore.Commands.Accounts.UpdateProfileSettings;

public sealed record UpdateProfileSettingsCommand(Guid AccountId, string? Bio) : ICommand<UpdateProfileSettingsResult>;

public sealed record UpdateProfileSettingsResult(string? Bio);
