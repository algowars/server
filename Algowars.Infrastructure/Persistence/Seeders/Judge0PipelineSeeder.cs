using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.ExecutionPipelines.Enums;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders;

/// <summary>
/// Ensures the default Judge0 execution pipeline exists in the database.
/// Problem seeders call <see cref="GetOrCreateAsync"/> to resolve the pipeline id
/// before seeding problem setups.
/// </summary>
internal sealed class Judge0PipelineSeeder(AlgowarsDbContext context) : ISeeder
{
    public const string PipelineName = "Default Judge0 Pipeline";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
        => await GetOrCreateAsync(cancellationToken);

    public async Task<Guid> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        Guid? existingId = await context.ExecutionPipelines
            .AsNoTracking()
            .Where(p => p.Name == PipelineName)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingId is not null)
            return existingId.Value;

        var pipeline = new ExecutionPipeline(PipelineName,
            "Executes submissions via Judge0: submit → poll → evaluate.");

        pipeline.AddStep(
            stepType: ExecutionPipelineStepType.Judge0Execute,
            stepOrder: 1,
            maxAttempts: 3,
            timeoutSeconds: 30,
            isPolling: false,
            name: "Submit to Judge0");

        pipeline.AddStep(
            stepType: ExecutionPipelineStepType.Judge0Poll,
            stepOrder: 2,
            maxAttempts: 10,
            timeoutSeconds: 10,
            isPolling: true,
            name: "Poll Judge0 Results");

        pipeline.AddStep(
            stepType: ExecutionPipelineStepType.Evaluate,
            stepOrder: 3,
            maxAttempts: 1,
            timeoutSeconds: 10,
            isPolling: false,
            name: "Evaluate Results");

        context.ExecutionPipelines.Add(pipeline);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();

        return pipeline.Id;
    }
}
