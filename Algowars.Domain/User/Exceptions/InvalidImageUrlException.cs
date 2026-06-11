using Algowars.Domain.SeedWork;

namespace Algowars.Domain.User.Exceptions;

public sealed class InvalidImageUrlException(string reason) : DomainException($"Image URL is invalid: {reason}")
{
}