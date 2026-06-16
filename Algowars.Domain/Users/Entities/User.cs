using Algowars.Domain.SeedWork;
using Algowars.Domain.Users.Exceptions;
using Algowars.Domain.Users.ValueObjects;

namespace Algowars.Domain.Users.Entities;

public sealed class User : AggregateRoot
{
    private User() { }

    public User(Username username, string sub)
    {
        Username = username ?? throw new InvalidUsernameException("Username is required.");
        Sub = string.IsNullOrWhiteSpace(sub) ? throw new InvalidUserSubException() : sub;
    }

    public static User Reconstitute(
        Guid id,
        Username username,
        string sub,
        ImageUrl? imageUrl,
        Bio? bio,
        DateTime? usernameLastChangedAt,
        DateTime createdOn)
    {
        var user = new User(username, sub)
        {
            ImageUrl = imageUrl,
            Bio = bio,
            UsernameLastChangedAt = usernameLastChangedAt,
        };
        user.Id = id;
        user.CreatedOn = createdOn;
        return user;
    }

    public void ChangeUsername(Username username)
    {
        if (UsernameLastChangedAt.HasValue &&
            DateTime.UtcNow - UsernameLastChangedAt.Value < TimeSpan.FromDays(MaxDaysUntilUsernameChange))
            throw new UsernameCooldownException(UsernameLastChangedAt.Value);

        Username = username;
        UsernameLastChangedAt = DateTime.UtcNow;
    }

    public void UpdateBio(Bio? bio) => Bio = bio;

    public void UpdateImageUrl(ImageUrl? imageUrl) => ImageUrl = imageUrl;

    public Bio? Bio { get; private set; }
    public ImageUrl? ImageUrl { get; private set; }
    public string Sub { get; private set; } = null!;
    public Username Username { get; private set; } = null!;
    public DateTime? UsernameLastChangedAt { get; private set; }

    private static readonly int MaxDaysUntilUsernameChange = 30;
}
