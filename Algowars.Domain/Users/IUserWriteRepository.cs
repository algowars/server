using Algowars.Domain.SeedWork;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;

namespace Algowars.Domain.Users;

public interface IUserWriteRepository : IRepository<User>
{
    Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken = default);
    Task<User?> FindByUsername(Username username, CancellationToken cancellationToken = default);
}