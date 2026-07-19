using System.Linq;
using Algowars.Application.Events;
using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Enums;
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
    IDomainEventDispatcher domainEventDispatcher) : AbstractCommandHandler<CreateSubmissionCommand, Guid>(validator)
{
    private const int OfficialRandomCaseCount = 20;

    protected override async Task<Result<Guid>> HandleValidated(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> testCaseIds;
        if (request.Type == SubmissionType.Run)
        {
            var publicCaseIds = await testSuiteRepository.FindPublicTestCaseIdsByProblemSetupIdAsync(
                request.ProblemSetupId,
                cancellationToken);

            var customCaseIds = await testSuiteRepository.CreateAdHocTestCasesAsync(
                request.CustomTestCases?.Select(tc => (IReadOnlyCollection<string>)tc.Inputs).ToArray() ?? [],
                cancellationToken);

            testCaseIds = [.. publicCaseIds, .. customCaseIds];
        }
        else
        {
            testCaseIds = await testSuiteRepository.FindRandomHiddenTestCaseIdsByProblemSetupIdAsync(
                request.ProblemSetupId,
                OfficialRandomCaseCount,
                cancellationToken);
        }

        if (testCaseIds.Count == 0)
            return Result<Guid>.Error("No test cases are available for this submission mode.");

        var pipelineId = await testSuiteRepository.FindPipelineIdByProblemSetupIdAsync(
            request.ProblemSetupId, cancellationToken);

        if (pipelineId is null)
            return Result<Guid>.NotFound("Submission pipeline is not available.");

        var pipeline = await pipelineRepository.FindByIdWithStepsAsync(pipelineId.Value, cancellationToken);
        if (pipeline is null)
            return Result<Guid>.NotFound("Submission pipeline is not available.");

        var firstStep = pipeline.FirstStep();
        if (firstStep is null)
            return Result<Guid>.Error("Submission pipeline is temporarily unavailable. Please try again later.");

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

        return Result<Guid>.Success(submission.Id);
    }
}
