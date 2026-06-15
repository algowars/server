using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class InvalidBioException(string reason) : DomainException($"Bio is invalid: {reason}")
{
}
