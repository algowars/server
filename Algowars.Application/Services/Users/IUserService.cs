using Ardalis.Result;

namespace Algowars.Application.Services.Users;

public interface IUserService
{
    Task<Result<Guid>> UpsertUserAsync(string sub, string? imageUrl, string? username = null, CancellationToken cancellationToken = default);
}
