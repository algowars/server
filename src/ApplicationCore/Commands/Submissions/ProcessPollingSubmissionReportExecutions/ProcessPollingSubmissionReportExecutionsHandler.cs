using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollingSubmissionReportExecutions;

public sealed class ProcessPollingSubmissionReportExecutionsHandler(ISubmissionRepository submissionRepository, IValidator<ProcessPollingSubmissionReportExecutionsCommand> validator) : AbstractCommandHandler<ProcessPollingSubmissionReportExecutionsCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(ProcessPollingSubmissionReportExecutionsCommand request, CancellationToken cancellationToken)
    {
        await submissionRepository.ProcessPollingSubmissionReportExecutionsAsync(
            request.Submissions, cancellationToken);

        return Result.Success();
    }
}
