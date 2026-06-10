
using Algowars.Domain.SeedWork;
using Algowars.Domain.User.Exceptions;
using Algowars.Domain.User.ValueObjects;

namespace Algowars.Domain.User.Entities;

public sealed class User(Username username, string sub) : AggregateRoot
{
    public Username Username { get; private set; } = username ?? throw new InvalidUsernameException("Username is required.");

    public string Sub { get; private set; } = string.IsNullOrWhiteSpace(sub)
            ? throw new InvalidUserSubException()
            : sub;

    public void ChangeUsername(Username username)
    {
        Username = username;
    }
}
