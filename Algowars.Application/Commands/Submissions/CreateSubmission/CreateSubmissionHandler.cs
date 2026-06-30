using Algowars.Application.Messaging;
using Algowars.Application.Messaging.Messages;
using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Factories;
using Algowars.Domain.Submissions.ValueObjects;
using Algowars.Domain.TestSuites;
using ApplicationCore.Commands;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

internal sealed partial class CreateSubmissionHandler(
    IValidator<CreateSubmissionCommand> validator,
    IAggregateFactory<Submission, CreateSubmissionParams> submissionFactory,
    ISubmissionWriteRepository submissionRepository,
    ITestSuiteWriteRepository testSuiteRepository,
    IMessagePublisher messagePublisher) : AbstractCommandHandler<CreateSubmissionCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var testCaseIds = await testSuiteRepository.FindTestCaseIdsByProblemSetupIdAsync(
            request.ProblemSetupId, cancellationToken);

        var submission = submissionFactory.Create(new CreateSubmissionParams(
            request.CreatedById,
            request.ProblemSetupId,
            request.Type,
            new SourceCode(request.Code),
            testCaseIds));

        await submissionRepository.AddAsync(submission, cancellationToken);

        await messagePublisher.PublishAsync(new SubmissionCreatedMessage(submission.Id), cancellationToken);

        return Result.Success();
    }
}
