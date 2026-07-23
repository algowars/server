using Algowars.Application.Commands.Submissions.CreateSubmission;
using Algowars.Application.Pagination;
using Algowars.Application.Queries.Submissions.GetSubmissionsByProblemSlug;
using Algowars.Application.Queries.Submissions.GetSubmissionStatus;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Submissions;

public interface ISubmissionService
{
    Task<Result<Guid>> CreateSubmissionAsync(CreateSubmissionDto dto, CancellationToken cancellationToken);

    Task<Result<SubmissionStatusDto>> GetSubmissionStatusAsync(Guid submissionId, Guid userId, CancellationToken cancellationToken);

    Task<Result<PageResult<ProblemSubmissionDto>>> GetSubmissionsByProblemSlugAsync(string slug,PaginationRequest paginationRequest, Guid? userId, bool includeAllSubmissions, CancellationToken cancellationToken);
}

internal sealed class SubmissionService(IMediator mediator) : ISubmissionService
{
    public async Task<Result<Guid>> CreateSubmissionAsync(CreateSubmissionDto dto, CancellationToken cancellationToken)
    {
        return await mediator.Send(
            new CreateSubmissionCommand(
                dto.ProblemSetupId,
                dto.Type,
                dto.Code,
                dto.CreatedById,
                dto.CustomTestCases),
            cancellationToken);
    }

    public async Task<Result<PageResult<ProblemSubmissionDto>>> GetSubmissionsByProblemSlugAsync(string slug, PaginationRequest paginationRequest, Guid? userId, bool includeAllSubmissions, CancellationToken cancellationToken)
    {
        return await mediator.Send(
            new GetSubmissionsByProblemSlugQuery(slug, paginationRequest, userId, includeAllSubmissions), cancellationToken);
    }

    public async Task<Result<SubmissionStatusDto>> GetSubmissionStatusAsync(Guid submissionId, Guid userId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetSubmissionStatusQuery(submissionId, userId), cancellationToken);
    }
}
