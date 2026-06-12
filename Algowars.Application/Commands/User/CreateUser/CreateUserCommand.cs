namespace Algowars.Application.Commands.User.CreateUser;

internal sealed record CreateUserCommand(string Username, string Sub, string? ImageUrl) : ICommand<Guid>;
