using Algowars.Application.Commands.Submissions.CreateSubmission;
using Algowars.Application.Queries.Submissions.GetSubmissionStatus;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Submissions;

public interface ISubmissionService
{
    Task<Result<Guid>> CreateSubmissionAsync(CreateSubmissionDto dto, CancellationToken cancellationToken);

    Task<Result<SubmissionStatusDto>> GetSubmissionStatusAsync(Guid submissionId, Guid userId, CancellationToken cancellationToken);
}

internal sealed class SubmissionService(IMediator mediator) : ISubmissionService
{
    public async Task<Result<Guid>> CreateSubmissionAsync(CreateSubmissionDto dto, CancellationToken cancellationToken)
    {
        return await mediator.Send(
            new CreateSubmissionCommand(dto.ProblemSetupId, dto.Type, dto.Code, dto.CreatedById),
            cancellationToken);
    }

    public async Task<Result<SubmissionStatusDto>> GetSubmissionStatusAsync(Guid submissionId, Guid userId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetSubmissionStatusQuery(submissionId, userId), cancellationToken);
    }
}
