using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Dtos.Problems.Tests;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Problems.GetProblemSetup;

public sealed class GetProblemSetupHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetProblemSetupQuery, ProblemSetupDto>
{
    private readonly IProblemRepository _problemRepository =
        problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));

    public async Task<Result<ProblemSetupDto>> Handle(
        GetProblemSetupQuery request,
        CancellationToken cancellationToken
    )
    {
        var setup = (
            await _problemRepository.GetProblemSetupAsync(
                request.ProblemId,
                request.LanguageVersionId,
                cancellationToken
            )
        );

        if (setup is null)
        {
            return Result.NotFound();
        }

        return new ProblemSetupDto()
        {
            Id = setup.Id,
            Version = setup.Version,
            InitialCode = setup.InitialCode,
            LanguageVersionId = setup.LanguageVersionId,
            TestSuites = setup
                .TestSuites.Where(ts => ts.TestSuiteType == TestSuiteType.Public)
                .Select(ts => new TestSuiteDto()
                {
                    TestCases = ts
                        .TestCases.Where(tc => tc.TestCaseType == TestCaseType.Sample)
                        .Select(tc => new TestCaseDto()
                        {
                            Input = tc.Input,
                            ExpectedOutput = tc.ExpectedOutput,
                        }),
                }),
        };
    }
}