using ApplicationCore.Domain.CodeExecution.Judge0;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Clients;

public interface IJudge0Client
{
    Task<Result<IEnumerable<Judge0SubmissionResponse>>> GetAsync(
        IEnumerable<Guid> tokens,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<Judge0SubmissionResponse>>> SubmitAsync(
        IEnumerable<Judge0SubmissionRequest> reqs,
        CancellationToken cancellationToken
    );
}
