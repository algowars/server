using Algowars.Application.Messaging;
using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.ValueObjects;
using Ardalis.Result;
using FluentValidation;
using MassTransit;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

internal sealed class CreateSubmissionHandler(
    IValidator<CreateSubmissionCommand> validator,
    ISubmissionRepository submissionRepository,
    IPublishEndpoint publishEndpoint)
    : AbstractCommandHandler<CreateSubmissionCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var sourceCode = new SourceCode(request.SourceCode);
        var submission = new Submission(
            request.UserId,
            request.ProblemVersionId,
            request.LanguageVersionId,
            request.Type,
            sourceCode,
            request.TestCaseIds);

        await submissionRepository.AddAsync(submission, cancellationToken);
        await publishEndpoint.Publish(new SubmissionCreatedMessage(submission.Id), cancellationToken);

        return Result.Success(submission.Id);
    }
}
