using Algowars.Domain.Users.Entities;

namespace Algowars.Application;

public sealed class UserContext
{
    public User? User { get; set; }

    public bool IsAuthenticated => User is not null;
}
