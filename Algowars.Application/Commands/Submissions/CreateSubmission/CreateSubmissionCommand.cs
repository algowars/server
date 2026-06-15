using Algowars.Domain.Submissions.Enums;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

public sealed record CreateSubmissionCommand(
    Guid UserId,
    Guid ProblemVersionId,
    Guid LanguageVersionId,
    SubmissionType Type,
    string SourceCode,
    IEnumerable<Guid> TestCaseIds) : ICommand<Guid>;
