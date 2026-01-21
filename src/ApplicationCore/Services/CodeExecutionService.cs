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
        if (!contextList.Any())
            return Result.Success(Enumerable.Empty<SubmissionModel>());

        var submissions = contextList.ToDictionary(
            c => c.SubmissionId,
            c => SubmissionModel.Create(c.SubmissionId, c.CreatedById)
        );

        var requests = new List<Judge0SubmissionRequest>();
        var map = new List<PendingResult>();

        foreach (var context in contextList)
        {
            var submission = submissions[context.SubmissionId];

            var built = context.BuiltResults.ToList();
            for (int i = 0; i < built.Count; i++)
            {
                var br = built[i];

                requests.Add(
                    new Judge0SubmissionRequest
                    {
                        LanguageId = br.LanguageVersionId,
                        SourceCode = br.SourceCode,
                        StdIn = br.Inputs,
                        ExpectedOutput = br.ExpectedOutput,
                    }
                );

                map.Add(new PendingResult(context.SubmissionId, i));

                submission.AddResult(
                    new SubmissionResult
                    {
                        Status = SubmissionStatus.InQueue,
                        StartedAt = DateTime.UtcNow,
                    }
                );
            }
        }

        var submitResult = await judge0Client.SubmitAsync(requests, cancellationToken);

        if (!submitResult.IsSuccess)
        {
            return Result.Error(submitResult.Errors.ToString());
        }

        var tokens = submitResult.Value.ToList();

        if (tokens.Count != map.Count)
        {
            return Result.Error("Judge0 token count mismatch.");
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            var pending = map[i];
            var submission = submissions[pending.SubmissionId];

            submission.Results[pending.ResultIndex].Id = tokens[i].Token;
        }

        return Result.Success<IEnumerable<SubmissionModel>>(submissions.Values);
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
            submissionResult.RuntimeMs = result.RuntimeMs.HasValue
                ? (int?)Math.Ceiling(result.RuntimeMs.Value)
                : null;
            submissionResult.MemoryKb = result.MemoryKb;
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
