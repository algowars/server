using Ardalis.Result;

namespace Algowars.Application.Services.Users;

public interface IUserService
{
    Task<Result<Guid>> CreateUserAsync(string username, string sub, string? imageUrl, CancellationToken cancellationToken = default);
}
