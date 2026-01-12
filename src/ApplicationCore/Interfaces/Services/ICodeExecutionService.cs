using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeExecutionService
{
    public Task<Result<SubmissionModel>> ExecuteAsync(
        CodeExecutionContext context,
        CancellationToken cancellationToken
    );
}
