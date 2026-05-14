using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissionStatus;

public sealed class GetSubmissionStatusHandler(
    ISubmissionRepository submissionRepository,
    IProblemRepository problemRepository
) : IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusDto>
{
    public async Task<Result<SubmissionStatusDto>> Handle(GetSubmissionStatusQuery request, CancellationToken cancellationToken)
    {
        var submission = await submissionRepository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.NotFound();
        }

        var results = submission.Results.ToList();

        if (results.Count == 0)
        {
            return Result.Success(new SubmissionStatusDto
            {
                SubmissionId = submission.Id,
                Status = submission.GetOverallStatus().ToString(),
                TestCases = [],
            });
        }

        var setups = await problemRepository.GetProblemSetupsAsync([submission.ProblemSetupId], cancellationToken);
        var setup = setups.FirstOrDefault();

        List<TestCaseModel> testCases = setup is not null
            ? setup.TestSuites.SelectMany(ts => ts.TestCases).ToList()
            : [];

        var testCaseDtos = results.Select((result, i) =>
        {
            var testCase = i < testCases.Count ? testCases[i] : null;

            return new SubmissionTestCaseResultDto
            {
                Input = testCase is not null
                    ? string.Join(",", testCase.Inputs.Select(inp => inp.Value.Trim()))
                    : string.Empty,
                ExpectedOutput = testCase?.ExpectedOutput.Value ?? string.Empty,
                ActualOutput = result.ProgramOutput ?? result.Stdout ?? string.Empty,
                ErrorOutput = result.Stderr,
                Status = (int)result.Status,
            };
        });

        return Result.Success(new SubmissionStatusDto
        {
            SubmissionId = submission.Id,
            Status = submission.GetOverallStatus().ToString(),
            TestCases = testCaseDtos.ToList(),
        });
    }
}