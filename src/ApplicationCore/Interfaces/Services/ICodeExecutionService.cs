using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeExecutionService
{
    Task<Result<SubmissionModel>> ExecuteAsync(
        CodeExecutionContext context,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<SubmissionResult>>> GetSubmissionResultsAsync(
        SubmissionModel submission,
        CancellationToken cancellationToken
    );
}
