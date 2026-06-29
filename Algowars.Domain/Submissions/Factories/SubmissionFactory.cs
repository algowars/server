using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Enums;
using Algowars.Domain.Submissions.ValueObjects;

namespace Algowars.Domain.Submissions.Factories;

public sealed record CreateSubmissionParams(
    Guid UserId,
    Guid ProblemVersionId,
    Guid LanguageVersionId,
    SubmissionType Type,
    SourceCode SourceCode,
    IEnumerable<Guid> TestCaseIds);

public sealed class SubmissionFactory : IAggregateFactory<Submission, CreateSubmissionParams>
{
    public Submission Create(CreateSubmissionParams parameters)
    {
        return new Submission(
            parameters.UserId,
            parameters.ProblemVersionId,
            parameters.LanguageVersionId,
            parameters.Type,
            parameters.SourceCode,
            parameters.TestCaseIds);
    }
}
