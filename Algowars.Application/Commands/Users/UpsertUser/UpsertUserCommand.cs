namespace Algowars.Application.Commands.Users.UpsertUser;

public sealed record UpsertUserCommand(string Sub, string? ImageUrl, string? Username) : ICommand<Guid>;
