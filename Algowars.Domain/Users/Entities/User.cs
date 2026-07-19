using Algowars.Domain.SeedWork;
using Algowars.Domain.Users.Events;
using Algowars.Domain.Users.Exceptions;
using Algowars.Domain.Users.ValueObjects;

namespace Algowars.Domain.Users.Entities;

public sealed class User(Username username, string sub) : AggregateRoot
{
    public void ChangeUsername(Username username)
    {
        if (UsernameLastChangedAt.HasValue &&
            DateTime.UtcNow - UsernameLastChangedAt.Value < TimeSpan.FromDays(MaxDaysUntilUsernameChange))
            throw new UsernameCooldownException(UsernameLastChangedAt.Value);

        Username = username;
        UsernameLastChangedAt = DateTime.UtcNow;
    }

    public void UpdateBio(Bio? bio)
    {
        Bio = bio;
    }

    public void UpdateImageUrl(ImageUrl? imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void MarkCreated() => AddDomainEvent(new UserCreatedDomainEvent(Id));

    public Bio? Bio { get; private set; }

    public ImageUrl? ImageUrl { get; private set; }

    public string Sub { get; private set; } = string.IsNullOrWhiteSpace(sub)
        ? throw new InvalidUserSubException()
        : sub;

    public Username Username { get; private set; } = username ?? throw new InvalidUsernameException("Username is required.");

    public DateTime? UsernameLastChangedAt { get; private set; }

    public static readonly int MaxDaysUntilUsernameChange = 30;
}