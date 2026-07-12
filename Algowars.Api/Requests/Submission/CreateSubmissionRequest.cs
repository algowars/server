using Algowars.Domain.Submissions.Enums;

namespace Algowars.Api.Requests.Submission;

public sealed record CreateSubmissionRequest(
    Guid ProblemSetupId,
    SubmissionType Type,
    string Code);
