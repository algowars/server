using Algowars.Application.Commands.Submissions.CreateSubmission;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Submissions;

public interface ISubmissionService
{
    Task<Result<Unit>> CreateSubmissionAsync(CreateSubmissionDto dto, CancellationToken cancellationToken);
}

internal sealed class SubmissionService(IMediator mediator) : ISubmissionService
{
    public async Task<Result<Unit>> CreateSubmissionAsync(CreateSubmissionDto dto, CancellationToken cancellationToken)
    {
        return await mediator.Send(
            new CreateSubmissionCommand(dto.ProblemSetupId, dto.Type, dto.Code, dto.CreatedById),
            cancellationToken);
    }
}
