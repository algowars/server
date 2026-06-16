namespace Algowars.Application.Commands.Users.CreateUser;

public sealed record CreateUserCommand(string Sub, string? ImageUrl) : ICommand<Guid>;
