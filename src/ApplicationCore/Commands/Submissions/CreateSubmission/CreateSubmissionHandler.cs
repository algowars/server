using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Messaging;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Logging;
using ApplicationCore.Messaging;
using Ardalis.Result;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed partial class CreateSubmissionHandler(
    ISubmissionRepository submissionRepository,
    IMessagePublisher messagePublisher,
    IValidator<CreateSubmissionCommand> validator,
    ILogger<CreateSubmissionHandler> logger
) : AbstractCommandHandler<CreateSubmissionCommand, Guid>(validator)
{
    private readonly ILogger<CreateSubmissionHandler> _logger = logger;
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

        try
        {
            var outboxId = await submissionRepository.SaveAsync(submission, cancellationToken);

            await messagePublisher.PublishAsync(
                new SubmissionCreatedMessage { SubmissionId = submission.Id, OutboxId = outboxId },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            LogCreateFailed(submission.Id, request.ProblemSetupId, request.CreatedById, ex);
            return Result<Guid>.Error("Unexpected error creating submission.");
        }

        LogCreated(submission.Id, request.ProblemSetupId, request.CreatedById);
        return Result.Success(submission.Id);
    }

    [LoggerMessage(
        EventId = LoggingEventIds.Submissions.Created,
        Level = LogLevel.Information,
        Message = "Submission {submissionId} created for setup {problemSetupId} by account {createdById}"
    )]
    private partial void LogCreated(Guid submissionId, int problemSetupId, Guid createdById);

    [LoggerMessage(
        EventId = LoggingEventIds.Submissions.CreateFailed,
        Level = LogLevel.Error,
        Message = "Failed to create submission for setup {problemSetupId} by account {createdById} (attempted id {submissionId})"
    )]
    private partial void LogCreateFailed(Guid submissionId, int problemSetupId, Guid createdById, Exception ex);
}