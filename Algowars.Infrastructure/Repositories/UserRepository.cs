using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class UserRepository(AlgoWarsDbContext context) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var model = ToModel(user);
        await context.Users.AddAsync(model, cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.Users.FindAsync([id], cancellationToken);
        return model is null ? null : ToDomain(model);
    }

    public async Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken)
    {
        var model = await context.Users.FirstOrDefaultAsync(u => u.Sub == sub, cancellationToken);
        return model is null ? null : ToDomain(model);
    }

    public async Task<User?> FindByUsername(Username username, CancellationToken cancellationToken)
    {
        var model = await context.Users.FirstOrDefaultAsync(u => u.Username == username.Value, cancellationToken);
        return model is null ? null : ToDomain(model);
    }

    private static UserDataModel ToModel(User user) => new()
    {
        Id = user.Id,
        Sub = user.Sub,
        Username = user.Username.Value,
        Bio = user.Bio?.Value,
        ImageUrl = user.ImageUrl?.Value,
        UsernameLastChangedAt = user.UsernameLastChangedAt
    };

    private static User ToDomain(UserDataModel model)
    {
        var user = new User(new Username(model.Username), model.Sub);

        if (model.Bio is not null)
            user.UpdateBio(new Bio(model.Bio));

        if (model.ImageUrl is not null)
            user.UpdateImageUrl(new ImageUrl(model.ImageUrl));

        return user;
    }
}

