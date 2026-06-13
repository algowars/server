namespace Algowars.Application.Commands.Users.CreateUser;

internal sealed record CreateUserCommand(string Username, string Sub, string? ImageUrl) : ICommand<Guid>;
