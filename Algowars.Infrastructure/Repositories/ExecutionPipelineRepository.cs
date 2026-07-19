using Algowars.Domain.ExecutionPipelines;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class ExecutionPipelineRepository(AlgowarsDbContext context)
    : IExecutionPipelineRepository
{
    public async Task AddAsync(ExecutionPipeline entity, CancellationToken cancellationToken = default)
    {
        await context.ExecutionPipelines.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExecutionPipeline?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ExecutionPipelines.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<ExecutionPipeline?> FindByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ExecutionPipelines
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task UpdateAsync(ExecutionPipeline entity, CancellationToken cancellationToken = default)
    {
        context.ExecutionPipelines.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
