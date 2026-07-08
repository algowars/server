using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class InvalidUserSubException : DomainException
{
    public InvalidUserSubException()
        : base("User sub is required.") { }
}