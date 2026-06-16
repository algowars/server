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
        db.Users.Add(new UserDataModel
        {
            Id = user.Id,
            Username = user.Username.Value,
            Sub = user.Sub,
            ImageUrl = user.ImageUrl?.Value,
            Bio = user.Bio?.Value,
            UsernameLastChangedAt = user.UsernameLastChangedAt,
            CreatedOn = user.CreatedOn,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        var model = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        if (model is null) return;

        model.Username = user.Username.Value;
        model.ImageUrl = user.ImageUrl?.Value;
        model.Bio = user.Bio?.Value;
        model.UsernameLastChangedAt = user.UsernameLastChangedAt;
        model.UpdatedOn = DateTime.UtcNow;

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
        var user = User.Reconstitute(
            model.Id,
            new Username(model.Username),
            model.Sub,
            model.ImageUrl is not null ? new ImageUrl(model.ImageUrl) : null,
            model.Bio is not null ? new Bio(model.Bio) : null,
            model.UsernameLastChangedAt,
            model.CreatedOn);
        return user;
    }
}
