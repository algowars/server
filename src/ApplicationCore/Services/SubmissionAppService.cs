using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Interfaces.Services;
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
}
