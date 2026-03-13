using ApplicationCore.Domain.CodeExecution;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessEvaluation;

public sealed record ProcessEvaluationCommand(IEnumerable<ComparisonContext> Contexts)
    : ICommand<Unit>;
