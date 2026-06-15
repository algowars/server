using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class InvalidUserSubException() : DomainException("User sub is required.")
{
}
