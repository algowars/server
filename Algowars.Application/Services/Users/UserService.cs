using Algowars.Application.Commands.Users.UpsertUser;
using Algowars.Application.Queries.Users.GetUserBySub;
using Algowars.Application.Users.Dtos;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Users;

public interface IUserService
{
    Task<Result<UserDto>> GetBySubAsync(string sub, CancellationToken cancellationToken);

    Task<Result<Unit>> UpsertAccountAsync(string sub, UpsertUserDto request, CancellationToken cancellationToken);
}


internal sealed class UserService(IMediator mediator) : IUserService
{
    public async Task<Result<UserDto>> GetBySubAsync(string sub, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetUserBySubQuery(sub), cancellationToken);
        return result;
    }

    public async Task<Result<Unit>> UpsertAccountAsync(string sub, UpsertUserDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpsertUserCommand(sub, request.Username, request.ImageUrl, request.Bio), cancellationToken);
        return result;
    }
}