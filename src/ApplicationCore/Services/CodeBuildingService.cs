using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;

namespace ApplicationCore.Services;

public sealed class CodeBuildingService(ICodeBuilderService codeBuilderService)
    : ICodeBuildingService
{
    public Result<IEnumerable<CodeExecutionContext>> BuildExecutionContexts(
        IEnumerable<SubmissionOutboxModel> outboxes,
        IDictionary<int, ProblemSetupModel> setupsMap
    )
    {
        var executionContexts = new List<CodeExecutionContext>();

        foreach (var outbox in outboxes)
        {
            if (!setupsMap.TryGetValue(outbox.Submission.ProblemSetupId, out var setup))
            {
                return Result<IEnumerable<CodeExecutionContext>>.Error(
                    $"Setup with id {outbox.Submission.ProblemSetupId} not found."
                );
            }

            var builderContexts = setup
                .TestSuites.SelectMany(ts => ts.TestCases)
                .Select(tc => new CodeBuilderContext
                {
                    Code = outbox.Submission.Code ?? "",
                    Template = setup.HarnessTemplate?.Template ?? "",
                    FunctionName = setup.FunctionName ?? string.Empty,
                    LanguageVersionId = setup.LanguageVersionId,
                    Inputs = tc.Input,
                    ExpectedOutput = tc.ExpectedOutput,
                });

            var buildResults = codeBuilderService.Build(builderContexts);

            if (!buildResults.IsSuccess)
            {
                return Result<IEnumerable<CodeExecutionContext>>.Error(
                    string.Join(", ", buildResults.Errors)
                );
            }

            executionContexts.Add(
                new CodeExecutionContext
                {
                    SubmissionId = outbox.SubmissionId,
                    Setup = setup,
                    Code = outbox.Submission.Code ?? "",
                    CreatedById = outbox.Submission.CreatedById,
                    BuiltResults = buildResults.Value,
                }
            );
        }

        return Result<IEnumerable<CodeExecutionContext>>.Success(executionContexts);
    }
}
