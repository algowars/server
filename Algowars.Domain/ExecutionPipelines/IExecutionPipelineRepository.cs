using Algowars.Domain.SeedWork;

namespace Algowars.Domain.ExecutionPipelines;

public interface IExecutionPipelineRepository : IRepository<ExecutionPipeline>
{
    Task<ExecutionPipeline?> FindByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
}
