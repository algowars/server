using Algowars.Application.Users.Dtos;

namespace Algowars.Application.Users;

public interface IUserReadRepository
{
    Task<UserDto?> FindBySubAsync(string sub, CancellationToken cancellationToken);
    Task<UserDto?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
