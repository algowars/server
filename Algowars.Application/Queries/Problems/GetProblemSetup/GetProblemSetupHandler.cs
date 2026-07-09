using Algowars.Application.Problems;
using Algowars.Application.Problems.Dtos;
using Ardalis.Result;

namespace Algowars.Application.Queries.Problems.GetProblemSetup;

internal sealed class GetProblemSetupHandler(IProblemReadRepository problemReadRepository) : IQueryHandler<GetProblemSetupQuery, ProblemSetupDto>
{
    public async Task<Result<ProblemSetupDto>> Handle(GetProblemSetupQuery request, CancellationToken cancellationToken)
    {
        var problem = await problemReadRepository.FindBySlugAsync(request.Slug, cancellationToken);

        if (problem is null)
        {
            return Result.NotFound();
        }

        var foundSetup = problem.FindSetupByLanguageVersionId(request.LanguageVersionId);

        if (foundSetup is null)
        {
            return Result.NotFound();
        }

        return Result.Success(new ProblemSetupDto(
            foundSetup.Id,
            foundSetup.InitialCode,
            foundSetup.PublicTestSuites().SelectMany(testSuite => testSuite.TestCases, (testSuite, testCase) => new ProblemSetupTestCaseDto(
                string.Join(", ", testCase.Inputs),
                string.Join(", ", testCase.ExpectedOutputs)
            ))
        ));
    }
}