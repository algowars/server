using Ardalis.Result;
using Algowars.Domain.Users.Entities;

namespace Algowars.Application.Services.Users;

public interface IUserService
{
    Task<Result<Guid>> CreateUserAsync(string username, string sub, string? imageUrl, CancellationToken cancellationToken = default);
    Task<Result<User>> GetBySubAsync(string sub, CancellationToken cancellationToken = default);
}
