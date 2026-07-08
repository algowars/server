using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Submissions.Exceptions;

public sealed class InvalidSubmissionStateException : DomainException
{
    public InvalidSubmissionStateException(string reason)
        : base($"Submission state transition is invalid: {reason}") { }
}
