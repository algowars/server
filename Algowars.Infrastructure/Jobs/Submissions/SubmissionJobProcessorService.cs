using Algowars.Application.ExecutionEngine;
using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.SubmissionJobs;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Jobs.Submissions;

/// <summary>
/// Core orchestration logic: picks up a pending <see cref="SubmissionJob"/>,
/// finds the current pipeline step, runs the appropriate <see cref="IStepHandler"/>,
/// then advances or completes the job.
/// </summary>
internal sealed partial class SubmissionJobProcessorService(
    ISubmissionJobRepository jobRepository,
    IExecutionPipelineRepository pipelineRepository,
    IStepHandlerRegistry handlerRegistry,
    ILogger<SubmissionJobProcessorService> logger)
{
    private const int BatchSize = 20;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.FindPendingAsync(BatchSize, cancellationToken);

        LogFound(jobs.Count);

        foreach (var job in jobs)
        {
            try
            {
                await ProcessJobAsync(job, cancellationToken);
            }
            catch (Exception ex)
            {
                LogJobFailed(job.Id, ex);
                job.Fail($"Unhandled exception: {ex.Message}");
                await jobRepository.UpdateAsync(job, cancellationToken);
            }
        }
    }

    private async Task ProcessJobAsync(SubmissionJob job, CancellationToken ct)
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

        // Enforce max-attempt guard
        int priorAttempts = job.AttemptCountForCurrentStep();
        if (priorAttempts >= step.MaxAttempts)
        {
            job.Fail($"Step '{step.Name}' exceeded max attempts ({step.MaxAttempts}).");
            await jobRepository.UpdateAsync(job, ct);
            return;
        }

        var attempt = job.StartAttempt();

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
                LogStepRetrying(job.Id, step.Name, job.AttemptCountForCurrentStep(), step.MaxAttempts);
            }
        }

        await jobRepository.UpdateAsync(job, ct);
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
}
