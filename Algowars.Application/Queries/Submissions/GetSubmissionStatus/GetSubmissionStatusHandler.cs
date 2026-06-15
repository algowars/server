using Algowars.Application.Dtos.Submissions;
using Algowars.Domain.Submissions;
using Ardalis.Result;

namespace Algowars.Application.Queries.Submissions.GetSubmissionStatus;

internal sealed class GetSubmissionStatusHandler(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusDto>
{
    public async Task<Result<SubmissionStatusDto>> Handle(GetSubmissionStatusQuery request, CancellationToken cancellationToken)
    {
        var submission = await submissionRepository.FindByIdAsync(request.SubmissionId, cancellationToken);
        if (submission is null)
            return Result.NotFound();

        var results = submission.Results.Select(r => new SubmissionResultDto
        {
            Id = r.Id,
            Status = r.Status.ToString(),
            RuntimeMs = r.Runtime,
            MemoryKb = r.MemoryUsed,
            ActualOutput = r.ActualOutput,
            StandardError = r.StandardError,
            CompileOutput = r.CompileOutput,
        });

        return Result.Success(new SubmissionStatusDto
        {
            SubmissionId = submission.Id,
            Status = submission.Status.ToString(),
            Results = results.ToList(),
        });
    }
}
