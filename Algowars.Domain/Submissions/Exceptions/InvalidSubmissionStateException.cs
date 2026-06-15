using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Submissions.Exceptions;

public sealed class InvalidSubmissionStateException(string reason)
    : DomainException($"Submission state transition is invalid: {reason}")
{
}
