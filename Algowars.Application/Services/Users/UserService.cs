using Algowars.Application.Commands.Users.CreateUser;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Users;

internal sealed class UserService(ISender sender) : IUserService
{
    public async Task<Result<Guid>> CreateUserAsync(string username, string sub, string? imageUrl, CancellationToken cancellationToken = default)
    {
        return await sender.Send(new CreateUserCommand(username, sub, imageUrl), cancellationToken);
    }
}
