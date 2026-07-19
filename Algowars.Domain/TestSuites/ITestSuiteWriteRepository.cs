using Algowars.Domain.SeedWork;
using Algowars.Domain.TestSuites.Entities;

namespace Algowars.Domain.TestSuites;

public interface ITestSuiteWriteRepository : IRepository<TestSuite>
{
    Task<IReadOnlyList<Guid>> FindTestCaseIdsByProblemSetupIdAsync(Guid problemSetupId, CancellationToken cancellationToken = default);
    Task<Guid?> FindPipelineIdByProblemSetupIdAsync(Guid problemSetupId, CancellationToken cancellationToken = default);
}
