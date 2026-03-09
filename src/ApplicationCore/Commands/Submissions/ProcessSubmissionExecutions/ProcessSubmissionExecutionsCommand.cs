using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;

public sealed record ProcessSubmissionExecutionsCommand(IEnumerable<SubmissionModel> Submissions)
    : ICommand<Unit>;
