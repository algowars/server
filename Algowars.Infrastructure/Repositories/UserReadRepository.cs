using Algowars.Application.Users;
using Algowars.Application.Users.Dtos;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class UserReadRepository(AlgowarsDbContext context) : IUserReadRepository
{
    public async Task<UserDto?> FindBySubAsync(string sub, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.Sub == sub)
            .Select(u => new UserDto(u.Id, u.Sub, u.Username.Value, u.ImageUrl != null ? u.ImageUrl.Value : null, UsernameLastChangedAt: u.UsernameLastChangedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDto?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDto(u.Id, u.Sub, u.Username.Value, u.ImageUrl != null ? u.ImageUrl.Value : null, UsernameLastChangedAt: u.UsernameLastChangedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
