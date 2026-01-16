using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Submissions.GetSubmission;
using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Services;

public sealed class SubmissionAppService(IMediator mediator) : ISubmissionAppService
{
    public async Task<Result<Guid>> ExecuteAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateSubmissionCommand(problemSetupId, code, createdById);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<SubmissionDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetSubmissionQuery(id);

        return await mediator.Send(query, cancellationToken);
    }
}
