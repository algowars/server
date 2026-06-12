using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;

namespace Algowars.Domain.Users;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    
    Task<User?> FindByUsrenameAsync(Username usrename, CancellationToken cancellationToken);

    Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken);
}
