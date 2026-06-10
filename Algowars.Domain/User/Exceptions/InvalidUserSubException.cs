using Algowars.Domain.SeedWork;

namespace Algowars.Domain.User.Exceptions;

public sealed class InvalidUserSubException : DomainException
{
    public InvalidUserSubException()
        : base("User sub is required.") { }
}