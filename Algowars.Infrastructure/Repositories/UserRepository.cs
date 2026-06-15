using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class UserRepository(AlgoWarsDbContext db) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var model = new UserDataModel
        {
            Id = user.Id,
            Username = user.Username.Value,
            Sub = user.Sub,
            ImageUrl = user.ImageUrl?.Value,
            Bio = user.Bio?.Value,
            UsernameLastChangedAt = user.UsernameLastChangedAt,
            CreatedOn = user.CreatedOn,
        };
        db.Users.Add(model);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return model is null ? null : MapToDomain(model);
    }

    public async Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken)
    {
        var model = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Sub == sub, cancellationToken);
        return model is null ? null : MapToDomain(model);
    }

    public async Task<User?> FindByUsername(Username username, CancellationToken cancellationToken)
    {
        var model = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.Value, cancellationToken);
        return model is null ? null : MapToDomain(model);
    }

    private static User MapToDomain(UserDataModel model)
    {
        var user = new User(new Username(model.Username), model.Sub);
        if (model.ImageUrl is not null) user.UpdateImageUrl(new ImageUrl(model.ImageUrl));
        if (model.Bio is not null) user.UpdateBio(new Bio(model.Bio));
        return user;
    }
}
