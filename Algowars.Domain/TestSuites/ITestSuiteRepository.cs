using Algowars.Domain.SeedWork;
using Algowars.Domain.TestSuites.Entities;

namespace Algowars.Domain.TestSuites;

public interface ITestSuiteRepository : IRepository<TestSuite>
{
    Task<IReadOnlyList<Guid>> FindTestCaseIdsByProblemSetupIdAsync(Guid problemSetupId, CancellationToken cancellationToken = default);
}
