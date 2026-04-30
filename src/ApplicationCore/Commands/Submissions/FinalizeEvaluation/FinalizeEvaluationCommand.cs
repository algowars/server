using MediatR;

namespace ApplicationCore.Commands.Submissions.FinalizeEvaluation;

public sealed record FinalizeEvaluationCommand(IEnumerable<Guid> OutboxIds, DateTime Now)
    : ICommand<Unit>;