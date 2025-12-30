using ApplicationCore.Domain.CodeExecution.Judge0;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Clients;

public interface IJudge0Client
{
    Task<Result<Judge0SubmissionResponse>> GetAsync(
        string token,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<Judge0SubmissionResponse>>> GetAsync(
        IEnumerable<string> tokens,
        CancellationToken cancellationToken
    );

    Task<Result<Judge0SubmissionResponse>> SubmitAsync(
        Judge0SubmissionRequest req,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<Judge0SubmissionResponse>>> SubmitAsync(
        IEnumerable<Judge0SubmissionRequest> reqs,
        CancellationToken cancellationToken
    );
}
