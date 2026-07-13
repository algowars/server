using System.Text.Json;
using Algowars.Application.ExecutionEngine;
using Algowars.Domain.ExecutionPipelines.Enums;
using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.ExecutionEngine.StepHandlers;

/// <summary>
/// Renders each test case through the language-specific code template,
/// submits the batch to Judge0, and stores the token→testCaseId map in the attempt payload.
/// </summary>
internal sealed partial class Judge0ExecuteStepHandler(
    AlgowarsDbContext db,
    IExecutionEngineStrategy engine,
    ICodeTemplateStrategyResolver templateResolver,
    ILogger<Judge0ExecuteStepHandler> logger) : IStepHandler
{
    public bool CanHandle(ExecutionPipelineStepType stepType)
        => stepType == ExecutionPipelineStepType.Judge0Execute;

    public async Task<StepHandlerResult> ExecuteAsync(StepHandlerContext context)
    {
        var job = context.Job;
        var ct = context.CancellationToken;

        // Load submission with results
        var submission = await db.Submissions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == job.SubmissionId, ct);

        if (submission is null)
            return Fail($"Submission {job.SubmissionId} not found.");

        // Load problem setup (language version id + function name + pipeline id)
        var setup = await db.Set<ProblemSetup>()
            .FirstOrDefaultAsync(ps => ps.Id == submission.ProblemSetupId, ct);

        if (setup is null)
            return Fail($"ProblemSetup {submission.ProblemSetupId} not found.");

        // Load the language version entry to get Judge0 language id + language slug
        var languageInfo = await (
            from lv in db.Set<LanguageVersionEntry>()
            join lang in db.Languages on EF.Property<Guid>(lv, "language_id") equals lang.Id
            where lv.Id == setup.LanguageVersionId
            select new { lv.Judge0Id, lang.Slug }
        ).FirstOrDefaultAsync(ct);

        if (languageInfo is null)
            return Fail($"LanguageVersionEntry {setup.LanguageVersionId} not found.");

        // Load test cases with inputs and expected outputs
        var testCaseIds = submission.Results.Select(r => r.TestCaseId).ToList();

        var testCases = await db.Set<TestCase>()
            .Include(tc => tc.Inputs)
            .Include(tc => tc.ExpectedOutputs)
            .Where(tc => testCaseIds.Contains(tc.Id))
            .ToListAsync(ct);

        if (testCases.Count == 0)
            return Fail("No test cases found for this submission.");

        // Resolve the code template strategy by language slug
        string languageSlug = languageInfo.Slug.Value;
        ICodeTemplateStrategy template;
        try
        {
            template = templateResolver.Resolve(languageSlug);
        }
        catch (InvalidOperationException ex)
        {
            return Fail(ex.Message);
        }

        // Build one execution submission per test case
        var submissions = testCases.Select(tc =>
        {
            var inputs = tc.Inputs
                .OrderBy(i => i.Id)
                .Select(i => new CodeTemplateInput(i.Value, i.ValueType))
                .ToList();

            string sourceCode = template.Render(new CodeTemplateContext(
                UserCode: submission.SourceCode.Value,
                FunctionName: setup.FunctionName,
                Inputs: inputs));

            string stdin = template.BuildStdin(inputs);

            return new ExecutionEngineSubmission(
                SourceCode: sourceCode,
                LanguageId: languageInfo.Judge0Id.Value,
                Stdin: stdin,
                TimeLimitMs: null,
                MemoryLimitKb: null);
        }).ToList();

        LogSubmitting(submissions.Count, job.SubmissionId);

        IReadOnlyList<ExecutionEngineResult> results;
        try
        {
            results = await engine.SubmitBatchAsync(submissions, ct);
        }
        catch (Exception ex)
        {
            return Fail($"Judge0 batch submit failed: {ex.Message}");
        }

        // Build a map: token -> testCaseId (by index) stored as the response payload
        var tokenMap = testCases
            .Zip(results, (tc, r) => new { tc.Id, r.Token })
            .ToDictionary(x => x.Token, x => x.Id);

        string responsePayload = JsonSerializer.Serialize(tokenMap);

        return new StepHandlerResult(Succeeded: true, ResponsePayload: responsePayload);
    }

    private static StepHandlerResult Fail(string error) => new(Succeeded: false, Error: error);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Submitting {Count} test cases to Judge0 for submission {SubmissionId}")]
    private partial void LogSubmitting(int count, Guid submissionId);
}
