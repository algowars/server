namespace Algowars.Api.Requests.Submission;

public sealed record CreateGradeSubmissionRequest(
    Guid ProblemSetupId,
    string Code);
