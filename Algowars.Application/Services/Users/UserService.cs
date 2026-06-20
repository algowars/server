using Algowars.Application.Commands.Users.CreateUser;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Users;

internal sealed class UserService(IMediator mediator, IUserRepository userRepository) : IUserService
{
    public async Task<Result<Guid>> CreateUserAsync(string username, string sub, string? imageUrl, CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new CreateUserCommand(username, sub, imageUrl), cancellationToken);
    }

    public async Task<Result<User>> GetBySubAsync(string sub, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindBySubAsync(sub, cancellationToken);
        return user is null ? Result.NotFound() : Result.Success(user);
    }
}
