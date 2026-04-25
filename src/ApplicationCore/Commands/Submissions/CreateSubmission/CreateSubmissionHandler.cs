using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Messaging;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Messaging;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed class CreateSubmissionHandler(
    ISubmissionRepository submissionRepository,
    IMessagePublisher messagePublisher,
    IValidator<CreateSubmissionCommand> validator
) : AbstractCommandHandler<CreateSubmissionCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(
        CreateSubmissionCommand request,
        CancellationToken cancellationToken
    )
    {
        var submission = new SubmissionModel
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            ProblemSetupId = request.ProblemSetupId,
            CreatedOn = DateTime.UtcNow,
            CreatedById = request.CreatedById,
        };

        var outboxId = await submissionRepository.SaveAsync(submission, cancellationToken);

        await messagePublisher.PublishAsync(
            new SubmissionCreatedMessage { SubmissionId = submission.Id, OutboxId = outboxId },
            cancellationToken
        );

        return Result.Success(submission.Id);
    }
}