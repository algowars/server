using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Submissions.Exceptions;

public sealed class SubmissionResultNotFoundException(Guid testCaseId)
    : DomainException($"Submission result for test case '{testCaseId}' was not found.")
{
}
