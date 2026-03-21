using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollingSubmissionExecutions;

public sealed class ProcessPollingSubmissionExecutionsHandler(ISubmissionRepository submissionRepository, IValidator<ProcessPollingSubmissionExecutionsCommand> validator) : AbstractCommandHandler<ProcessPollingSubmissionExecutionsCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(ProcessPollingSubmissionExecutionsCommand request, CancellationToken cancellationToken)
    {
        await submissionRepository.ProcessPollingSubmissionExecutionsAsync(
            request.Submissions, cancellationToken);

        return Result.Success();
    }
}