using Algowars.Application.ExecutionEngine;
using Algowars.Application.Languages;
using Algowars.Domain.ExecutionPipelines.Enums;
using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Problems;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Submissions;
using Algowars.Domain.TestSuites.Entities;
using System.Text.Json;

namespace Algowars.Infrastructure.ExecutionEngine.StepHandlers;

/// <summary>
/// Renders each test case through the language-specific code template,
/// submits the batch to Judge0, and stores the token→testCaseId map in the attempt payload.
/// </summary>
internal sealed partial class Judge0ExecutionStepHandler(
    ISubmissionWriteRepository submissionWriteRepository,
    IProblemRepository problemRepository,
    ILanguageReadRepository languageReadRepository,
    ICodeTemplateStrategyResolver templateResolver,
    IExecutionEngineStrategy executionEngine) : IStepHandler
{
    public bool CanHandle(ExecutionPipelineStepType stepType) => stepType == ExecutionPipelineStepType.Judge0Execute;

    public async Task<StepHandlerResult> ExecuteAsync(StepHandlerContext context)
    {
        var job = context.Job;
        var ct = context.CancellationToken;

        var submission = await submissionWriteRepository.FindByIdAsync(job.SubmissionId, ct);
        if (submission is null)
            return Fail($"Submission {job.SubmissionId} not found.");

        var (Setup, Error) = await ResolveSetupAsync(submission.ProblemSetupId, ct);
        if (Error is not null)
            return Fail(Error);

        var languageResult = await ResolveLanguageAsync(Setup!.LanguageVersionId, ct);
        if (languageResult.Error is not null)
            return Fail(languageResult.Error);

        var templateStrategy = templateResolver.Resolve(languageResult.Language!.Name.Value);

        var submissions = BuildSubmissions(
            Setup!,
            submission.SourceCode.Value,
            templateStrategy,
            languageResult.VersionEntry!.Judge0Id.Value);

        var results = await executionEngine.SubmitBatchAsync(submissions, ct);

        var tokenMap = BuildTokenMap([.. Setup!.TestSuites.SelectMany(ts => ts.TestCases)], results);

        return new StepHandlerResult(Succeeded: true, ResponsePayload: JsonSerializer.Serialize(tokenMap));
    }

    private async Task<(ProblemSetup? Setup, string? Error)> ResolveSetupAsync(Guid setupId, CancellationToken ct)
    {
        var problem = await problemRepository.FindBySetupIdAsync(setupId, ct);
        if (problem is null)
            return (null, $"Problem for setup {setupId} not found.");

        var setup = problem.Setups.FirstOrDefault(s => s.Id == setupId);
        if (setup is null)
            return (null, $"Setup {setupId} not found on problem.");

        return (setup, null);
    }

    private async Task<(Language? Language, LanguageVersionEntry? VersionEntry, string? Error)> ResolveLanguageAsync(Guid versionId, CancellationToken ct)
    {
        var languages = await languageReadRepository.FindLanguagesByVersionId([versionId], ct);
        var language = languages.FirstOrDefault();
        if (language is null)
            return (null, null, $"Language for version {versionId} not found.");

        var versionEntry = language.Versions.FirstOrDefault(v => v.Id == versionId);
        if (versionEntry is null)
            return (null, null, $"Language version entry {versionId} not found.");

        return (language, versionEntry, null);
    }

    private static List<ExecutionEngineSubmission> BuildSubmissions(
        ProblemSetup setup,
        string userCode,
        ICodeTemplateStrategy templateStrategy,
        int judge0LanguageId)
    {
        return [.. setup.TestSuites
            .SelectMany(ts => ts.TestCases)
            .Select(testCase =>
            {
                var inputs = testCase.Inputs
                    .Select(i => new CodeTemplateInput(i.Value, i.ValueType))
                    .ToList();

                var templateContext = new CodeTemplateContext(
                    UserCode: userCode,
                    FunctionName: setup.FunctionName,
                    Inputs: inputs);

                return new ExecutionEngineSubmission(
                    SourceCode: templateStrategy.Render(templateContext),
                    LanguageId: judge0LanguageId,
                    Stdin: templateStrategy.BuildStdin(inputs),
                    TimeLimitMs: null,
                    MemoryLimitKb: null);
            })];
    }

    private static Dictionary<string, Guid> BuildTokenMap(
        List<TestCase> testCases,
        IReadOnlyList<ExecutionEngineResult> results)
    {
        return testCases
            .Zip(results, (testCase, result) => new { testCase.Id, result.Token })
            .ToDictionary(x => x.Token, x => x.Id);
    }

    private static StepHandlerResult Fail(string error) => new(Succeeded: false, Error: error);
}
