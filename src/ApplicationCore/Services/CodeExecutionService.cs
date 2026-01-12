using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;

namespace ApplicationCore.Services;

public sealed class CodeExecutionService(IJudge0Client judge0Client) : ICodeExecutionService
{
    public async Task<Result<SubmissionModel>> ExecuteAsync(
        CodeExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        var submissionRequests = context
            .BuiltResults.Select(b => new Judge0SubmissionRequest
            {
                LanguageId = 102,
                SourceCode = b.FinalCode,
                StdIn = b.Inputs,
                ExpectedOutput = b.ExpectedOutputs,
            })
            .ToList();

        var judge0Result = await judge0Client.SubmitAsync(submissionRequests, cancellationToken);

        if (!judge0Result.IsSuccess)
        {
            return Result.Error("Failed to submit code for execution.");
        }

        var submission = new SubmissionModel
        {
            Id = Guid.NewGuid(),
            ProblemSetupId = context.Setup.Id,
            Code = context.Code,
            CreatedById = context.CreatedById,
            Results = judge0Result
                .Value.Select(result => new SubmissionResult
                {
                    Id = result.Token,
                    Status = MapJudge0SubmissionStatus(result.Status),
                    Stdout = result.Stdout,
                    Stderr = result.Stderr,
                    RuntimeMs = result.RuntimeMs,
                    MemoryKb = result.MemoryKb,
                })
                .ToList(),
        };

        return Result.Success<SubmissionModel>(submission);
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
