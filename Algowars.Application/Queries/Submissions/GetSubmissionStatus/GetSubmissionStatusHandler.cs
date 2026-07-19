using Algowars.Application.Submissions.Dtos;
using Algowars.Domain.Submissions;
using Algowars.Domain.TestSuites;
using Ardalis.Result;

namespace Algowars.Application.Queries.Submissions.GetSubmissionStatus;

internal sealed class GetSubmissionStatusHandler(
    ISubmissionWriteRepository submissionRepository,
    ITestSuiteWriteRepository testSuiteRepository)
    : IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusDto>
{
    public async Task<Result<SubmissionStatusDto>> Handle(GetSubmissionStatusQuery request, CancellationToken cancellationToken)
    {
        var submission = await submissionRepository.FindByIdAsync(request.SubmissionId, cancellationToken);

        if (submission is null || submission.UserId != request.UserId)
            return Result<SubmissionStatusDto>.NotFound();

        var expectedOutputMap = await testSuiteRepository.FindExpectedOutputsByTestCaseIdsAsync(
            submission.Results.Select(result => result.TestCaseId),
            cancellationToken);

        var results = submission.Results
            .Select(result => new SubmissionResultStatusDto(
                result.Status,
                result.Runtime,
                result.MemoryUsed,
                result.ActualOutput,
                expectedOutputMap.GetValueOrDefault(result.TestCaseId),
                result.StandardOutput,
                result.StandardError,
                result.CompileOutput))
            .ToArray();

        return Result<SubmissionStatusDto>.Success(new SubmissionStatusDto(
            submission.Id,
            submission.ProblemSetupId,
            submission.Status,
            results));
    }
}
