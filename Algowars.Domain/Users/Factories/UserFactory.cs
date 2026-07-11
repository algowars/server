using Algowars.Domain.SeedWork;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;

namespace Algowars.Domain.Users.Factories;

public sealed record CreateUserParams(string Username, string Sub, string? ImageUrl);

public sealed class UserFactory : IAggregateFactory<User, CreateUserParams>
{
    public User Create(CreateUserParams parameters)
    {
        var username = new Username(parameters.Username);
        var imageUrl = parameters.ImageUrl is not null ? new ImageUrl(parameters.ImageUrl) : null;
        var user = new User(username, parameters.Sub);
        user.UpdateImageUrl(imageUrl);
        return user;
    }
}