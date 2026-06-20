using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;

namespace Algowars.Domain.Users;

public interface IUserWriteRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken);
    Task<User?> FindByUsername(Username username, CancellationToken cancellationToken);
}
