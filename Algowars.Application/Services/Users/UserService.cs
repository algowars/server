using Algowars.Application.Commands.Users.UpsertUser;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Users;

internal sealed class UserService(ISender sender) : IUserService
{
    public async Task<Result<Guid>> UpsertUserAsync(string sub, string? imageUrl, string? username = null, CancellationToken cancellationToken = default)
        => await sender.Send(new UpsertUserCommand(sub, imageUrl, username), cancellationToken);
}
