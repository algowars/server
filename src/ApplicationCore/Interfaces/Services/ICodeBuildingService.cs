using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Submissions.Outboxes;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeBuildingService
{
    /// <summary>
    /// Given the initialized outboxes and their resolved problem setups,
    /// builds a <see cref="CodeExecutionContext"/> per outbox with all
    /// test-case build results populated.
    /// </summary>
    Result<IEnumerable<CodeExecutionContext>> BuildExecutionContexts(
        IEnumerable<SubmissionOutboxModel> outboxes,
        IDictionary<int, ProblemSetupModel> setupsMap
    );
}
