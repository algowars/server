using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface IComparisonBuildingService
{
    Result<IEnumerable<ComparisonContext>> BuildComparisonContexts(
        IEnumerable<SubmissionOutboxModel> outboxes
    );
}
