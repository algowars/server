using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;

namespace ApplicationCore.Services;

public sealed class ComparisonBuildingService : IComparisonBuildingService
{
    public Result<IEnumerable<ComparisonContext>> BuildComparisonContexts(
        IEnumerable<SubmissionOutboxModel> outboxes
    )
    {
        var contexts = new List<ComparisonContext>();

        foreach (var outbox in outboxes)
        {
            var builtResults = new List<ComparisonBuildResult>();

            foreach (var result in outbox.Submission.Results)
            {
                if (result.Stdout is null || result.ExpectedOutput is null)
                {
                    continue;
                }

                int lastNewlineIndex = result.Stdout.LastIndexOf('\n');
                string actualOutput =
                    lastNewlineIndex >= 0 ? result.Stdout[(lastNewlineIndex + 1)..] : result.Stdout;

                builtResults.Add(
                    new ComparisonBuildResult
                    {
                        ExecutionId = result.Id,
                        ActualOutput = actualOutput,
                        ExpectedOutput = result.ExpectedOutput,
                    }
                );
            }

            contexts.Add(
                new ComparisonContext
                {
                    SubmissionId = outbox.SubmissionId,
                    BuiltResults = builtResults,
                }
            );
        }

        return Result<IEnumerable<ComparisonContext>>.Success(contexts);
    }
}
