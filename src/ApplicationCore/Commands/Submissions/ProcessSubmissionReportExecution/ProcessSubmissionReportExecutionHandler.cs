using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionReportExecution;

public sealed class ProcessSubmissionReportExecutionHandler(
    ISubmissionRepository submissionRepository,
    IValidator<ProcessSubmissionReportExecutionCommand> validator
) : AbstractCommandHandler<ProcessSubmissionReportExecutionCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        ProcessSubmissionReportExecutionCommand request,
        CancellationToken cancellationToken
    )
    {
        await submissionRepository.ProcessSubmissionReportExecutionAsync(
            request.Submissions,
            cancellationToken
        );

        return Result.Success();
    }
}
