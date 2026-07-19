using Algowars.Application.Events;
using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Factories;
using Algowars.Domain.Submissions.ValueObjects;
using Algowars.Domain.SubmissionJobs;
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
    ISubmissionJobRepository submissionJobRepository,
    IExecutionPipelineRepository pipelineRepository,
    ITestSuiteWriteRepository testSuiteRepository,
    IDomainEventDispatcher domainEventDispatcher) : AbstractCommandHandler<CreateSubmissionCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var testCaseIds = await testSuiteRepository.FindTestCaseIdsByProblemSetupIdAsync(
            request.ProblemSetupId, cancellationToken);

        var pipelineId = await testSuiteRepository.FindPipelineIdByProblemSetupIdAsync(
            request.ProblemSetupId, cancellationToken);

        if (pipelineId is null)
            return Result.NotFound($"No pipeline configured for problem setup {request.ProblemSetupId}.");

        var pipeline = await pipelineRepository.FindByIdWithStepsAsync(pipelineId.Value, cancellationToken);
        if (pipeline is null)
            return Result.NotFound($"Pipeline {pipelineId.Value} not found.");

        var firstStep = pipeline.FirstStep();
        if (firstStep is null)
            return Result.Error($"Pipeline {pipelineId.Value} has no steps configured.");

        var submission = submissionFactory.Create(new CreateSubmissionParams(
            request.CreatedById,
            request.ProblemSetupId,
            request.Type,
            new SourceCode(request.Code),
            testCaseIds));

        await submissionRepository.AddAsync(submission, cancellationToken);

        var job = new SubmissionJob(submission.Id, pipeline.Id, firstStep.Id);
        await submissionJobRepository.AddAsync(job, cancellationToken);

        await domainEventDispatcher.DispatchAsync(submission.PopDomainEvents(), cancellationToken);

        return Result.Success();
    }
}
