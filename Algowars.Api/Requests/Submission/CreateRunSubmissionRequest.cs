namespace Algowars.Api.Requests.Submission;

public sealed record RunTestCaseInputRequest(
    IReadOnlyCollection<string> Inputs);

public sealed record CreateRunSubmissionRequest(
    Guid ProblemSetupId,
    string Code,
    IReadOnlyCollection<RunTestCaseInputRequest>? CustomTestCases);
