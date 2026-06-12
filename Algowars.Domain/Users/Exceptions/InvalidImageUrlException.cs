using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class InvalidImageUrlException(string reason) : DomainException($"Image URL is invalid: {reason}")
{
}