using Algowars.Application.ExecutionEngine;
using Algowars.Application.Messaging;
using Algowars.Application.Messaging.Messages;
using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.SubmissionJobs;
using Algowars.Domain.SubmissionJobs.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Jobs.Submissions;

/// <summary>
/// Picks up pending <see cref="SubmissionJob"/> entries and processes them.
/// Creates a new DI scope per job so each job gets its own isolated DbContext.
/// </summary>
internal sealed partial class SubmissionJobProcessorService(
    IServiceScopeFactory scopeFactory,
    ILogger<SubmissionJobProcessorService> logger)
{
    private const int BatchSize = 20;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<SubmissionJob> jobs;
        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISubmissionJobRepository>();
            jobs = await repo.FindPendingAsync(BatchSize, cancellationToken);
        }

        LogFound(jobs.Count);

        foreach (var job in jobs)
        {
            await ProcessInNewScopeAsync(job.Id, cancellationToken);
        }
    }

    public async Task RunForSubmissionAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        Guid? jobId;
        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISubmissionJobRepository>();
            var job = await repo.FindBySubmissionIdAsync(submissionId, cancellationToken);
            if (job is null)
            {
                LogSubmissionJobNotFound(submissionId);
                return;
            }

            if (job.Status is SubmissionJobStatus.Completed or SubmissionJobStatus.Failed)
            {
                LogJobAlreadyProcessed(job.Id);
                return;
            }

            jobId = job.Id;
        }

        await ProcessInNewScopeAsync(jobId.Value, cancellationToken);
    }

    /// <summary>
    /// Loads and processes a single job inside its own DI scope and DbContext.
    /// </summary>
    private async Task ProcessInNewScopeAsync(Guid jobId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var jobRepository = sp.GetRequiredService<ISubmissionJobRepository>();
        var pipelineRepository = sp.GetRequiredService<IExecutionPipelineRepository>();
        var handlerRegistry = sp.GetRequiredService<IStepHandlerRegistry>();
        var messagePublisher = sp.GetRequiredService<IMessagePublisher>();

        var job = await jobRepository.FindByIdAsync(jobId, ct);
        if (job is null)
        {
            logger.LogWarning("Job {JobId} not found when processing in isolated scope.", jobId);
            return;
        }

        if (job.Status is SubmissionJobStatus.Completed or SubmissionJobStatus.Failed)
        {
            LogJobAlreadyProcessed(job.Id);
            return;
        }

        await ProcessJobAsync(job, jobRepository, pipelineRepository, handlerRegistry, messagePublisher, ct);
    }

    private async Task ProcessJobAsync(
        SubmissionJob job,
        ISubmissionJobRepository jobRepository,
        IExecutionPipelineRepository pipelineRepository,
        IStepHandlerRegistry handlerRegistry,
        IMessagePublisher messagePublisher,
        CancellationToken ct)
    {
        if (job.CurrentStepId is null)
        {
            job.Complete();
            await jobRepository.UpdateAsync(job, ct);
            return;
        }

        var pipeline = await pipelineRepository.FindByIdWithStepsAsync(job.PipelineId, ct);
        if (pipeline is null)
        {
            job.Fail($"Pipeline {job.PipelineId} not found.");
            await jobRepository.UpdateAsync(job, ct);
            return;
        }

        var step = pipeline.Steps.FirstOrDefault(s => s.Id == job.CurrentStepId);
        if (step is null)
        {
            job.Fail($"Step {job.CurrentStepId} not found in pipeline {job.PipelineId}.");
            await jobRepository.UpdateAsync(job, ct);
            return;
        }

        int priorAttempts = job.AttemptCountForCurrentStep();
        if (priorAttempts >= step.MaxAttempts)
        {
            job.Fail($"Step '{step.Name}' exceeded max attempts ({step.MaxAttempts}).");
            await jobRepository.UpdateAsync(job, ct);
            return;
        }

        var attempt = job.StartAttempt();

        await jobRepository.PersistAttemptAsync(job, attempt, ct);

        IStepHandler handler;
        try
        {
            handler = handlerRegistry.Resolve(step.StepType);
        }
        catch (InvalidOperationException ex)
        {
            attempt.Fail(ex.Message);
            job.Fail(ex.Message);
            await jobRepository.UpdateAsync(job, ct);
            return;
        }

        LogExecutingStep(job.Id, step.Name, step.StepType.ToString());

        var result = await handler.ExecuteAsync(new StepHandlerContext(job, step, attempt, ct));

        if (result.Succeeded)
        {
            attempt.Succeed(result.RequestPayload, result.ResponsePayload);

            var nextStep = pipeline.NextStep(step.StepOrder);
            if (nextStep is not null)
            {
                job.AdvanceTo(nextStep.Id);
                LogAdvanced(job.Id, nextStep.Name);
            }
            else
            {
                job.Complete();
                LogCompleted(job.Id);
            }
        }
        else
        {
            attempt.Fail(result.Error ?? "Unknown error.", result.RequestPayload, result.ResponsePayload);

            bool hasRetriesLeft = job.AttemptCountForCurrentStep() < step.MaxAttempts;
            if (!hasRetriesLeft)
            {
                job.Fail($"Step '{step.Name}' failed after {step.MaxAttempts} attempt(s): {result.Error}");
                LogStepFailed(job.Id, step.Name, result.Error);
            }
            else
            {
                job.ResetToPending();
                LogStepRetrying(job.Id, step.Name, job.AttemptCountForCurrentStep(), step.MaxAttempts);
            }
        }

        await jobRepository.UpdateAsync(job, ct);

        // The job still has a step to run (advanced to the next step, or reset to
        // pending for a retry) — publish a message so it gets picked up immediately
        // instead of sitting idle until the fallback cron sweep runs.
        if (job.Status == SubmissionJobStatus.Pending)
        {
            // If this is a retry of the *same* polling step (e.g. waiting on Judge0),
            // back off for the step's configured interval before checking again.
            // Without this, retries fire back-to-back and burn through MaxAttempts
            // in a couple of seconds — long before Judge0 has actually finished.
            bool isRetryOfSameStep = job.CurrentStepId == step.Id;
            if (isRetryOfSameStep && step.IsPolling)
            {
                await Task.Delay(TimeSpan.FromSeconds(step.TimeoutSeconds), ct);
            }

            await messagePublisher.PublishAsync(
                new SubmissionJobContinuationMessage(job.SubmissionId), ct);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Found {Count} pending submission jobs.")]
    private partial void LogFound(int count);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Executing step '{StepName}' ({StepType}) for job {JobId}")]
    private partial void LogExecutingStep(Guid jobId, string stepName, string stepType);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Job {JobId} advanced to step '{StepName}'")]
    private partial void LogAdvanced(Guid jobId, string stepName);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Job {JobId} completed successfully.")]
    private partial void LogCompleted(Guid jobId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Job {JobId} step '{StepName}' failed permanently: {Error}")]
    private partial void LogStepFailed(Guid jobId, string stepName, string? error);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Job {JobId} step '{StepName}' attempt {Attempt}/{Max} failed; will retry.")]
    private partial void LogStepRetrying(Guid jobId, string stepName, int attempt, int max);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unhandled exception processing job {JobId}")]
    private partial void LogJobFailed(Guid jobId, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Job {JobId} was already processed by another worker — skipping.")]
    private partial void LogJobAlreadyProcessed(Guid jobId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No submission job found for submission {SubmissionId}.")]
    private partial void LogSubmissionJobNotFound(Guid submissionId);
}