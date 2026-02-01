using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeExecutionService
{
    Task<Result<IEnumerable<SubmissionModel>>> ExecuteAsync(
        IEnumerable<CodeExecutionContext> contexts,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<SubmissionModel>>> GetSubmissionResultsAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );
}
