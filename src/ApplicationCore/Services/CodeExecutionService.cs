using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;

namespace ApplicationCore.Services;

public sealed class CodeExecutionService(IJudge0Client judge0Client) : ICodeExecutionService
{
    public async Task<Result<IEnumerable<SubmissionModel>>> ExecuteAsync(
        IEnumerable<CodeExecutionContext> contexts,
        CancellationToken cancellationToken
    )
    {
        var contextList = contexts.ToList();
        if (contextList.Count == 0)
        {
            return Result.Success(Enumerable.Empty<SubmissionModel>());
        }

        var submissions = new List<SubmissionModel>();
        var judge0Requests = new List<Judge0SubmissionRequest>();
        var indexMap = new List<(List<SubmissionResult> Results, int ResultIndex)>();

        foreach (var context in contextList)
        {
            var results = new List<SubmissionResult>();

            foreach (var buildResult in context.BuiltResults)
            {
                judge0Requests.Add(
                    new Judge0SubmissionRequest
                    {
                        LanguageId = 102,
                        SourceCode = buildResult.FinalCode,
                        StdIn = buildResult.Inputs,
                        ExpectedOutput = buildResult.ExpectedOutput,
                    }
                );

                results.Add(new SubmissionResult { Status = SubmissionStatus.InQueue });

                indexMap.Add((results, results.Count - 1));
            }

            var submission = new SubmissionModel
            {
                Id = context.SubmissionId ?? Guid.NewGuid(),
                CreatedById = context.CreatedById,
                Results = results,
            };

            submissions.Add(submission);
        }

        if (!judge0Requests.Any())
        {
            return Result.Success<IEnumerable<SubmissionModel>>(submissions);
        }

        var judge0Response = await judge0Client.SubmitAsync(judge0Requests, cancellationToken);

        if (!judge0Response.IsSuccess)
        {
            return Result.Error("Failed to submit code for execution.");
        }

        var responseList = judge0Response.Value.ToList();

        if (responseList.Count != indexMap.Count)
        {
            return Result.Error("Mismatch between Judge0 responses and submitted jobs.");
        }

        for (int i = 0; i < responseList.Count; i++)
        {
            var response = responseList[i];
            (var results, int resultIndex) = indexMap[i];

            var result = results[resultIndex];
            result.Id = response.Token;
            result.Status = MapJudge0SubmissionStatus(response.Status);
        }

        return Result.Success<IEnumerable<SubmissionModel>>(submissions);
    }

    public async Task<Result<IEnumerable<SubmissionModel>>> GetSubmissionResultsAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    )
    {
        var submissionList = submissions.ToList();
        if (!submissionList.Any())
        {
            return Result.Success(Enumerable.Empty<SubmissionModel>());
        }

        var tokenMap = submissionList
            .SelectMany(s => s.Results.Select(r => (Submission: s, Token: r.Id)))
            .ToDictionary(x => x.Token, x => x.Submission);

        var judge0Results = await judge0Client.GetAsync(tokenMap.Keys, cancellationToken);

        if (!judge0Results.IsSuccess)
        {
            return Result.Error("Failed to retrieve submission results.");
        }

        foreach (var result in judge0Results.Value)
        {
            if (!tokenMap.TryGetValue(result.Token, out var submission))
            {
                continue;
            }

            var submissionResult = submission.Results.First(r => r.Id == result.Token);

            submissionResult.Status = MapJudge0SubmissionStatus(result.Status);
            submissionResult.Stdout = result.Stdout;
            submissionResult.RuntimeMs = decimal.TryParse(result.Time, out decimal seconds)
                ? (int)Math.Ceiling(seconds * 1000)
                : null;
            submissionResult.MemoryKb = result.Memory;
            submissionResult.FinishedAt = DateTime.UtcNow;
        }

        return Result.Success<IEnumerable<SubmissionModel>>(submissionList);
    }

    private static SubmissionStatus MapJudge0SubmissionStatus(Judge0StatusModel status) =>
        status.Id switch
        {
            1 => SubmissionStatus.InQueue,
            2 => SubmissionStatus.Processing,
            3 => SubmissionStatus.Accepted,
            4 => SubmissionStatus.WrongAnswer,
            5 => SubmissionStatus.TimeLimitExceeded,
            6 => SubmissionStatus.CompilationError,
            7 => SubmissionStatus.RuntimeErrorSigSegv,
            8 => SubmissionStatus.RuntimeErrorSigXfsz,
            9 => SubmissionStatus.RuntimeErrorSigFpe,
            10 => SubmissionStatus.RuntimeErrorSigAbrt,
            11 => SubmissionStatus.RuntimeErrorNzec,
            12 => SubmissionStatus.RuntimeErrorOther,
            13 => SubmissionStatus.InternalError,
            14 => SubmissionStatus.ExecFormatError,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status.Id),
                status.Id,
                "Unknown Judge0 submission status"
            ),
        };
}
