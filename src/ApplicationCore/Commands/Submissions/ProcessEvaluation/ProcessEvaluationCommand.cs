using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessEvaluation;

public sealed record ProcessEvaluationCommand(IEnumerable<SubmissionModel> Submissions)
    : ICommand<Unit>;