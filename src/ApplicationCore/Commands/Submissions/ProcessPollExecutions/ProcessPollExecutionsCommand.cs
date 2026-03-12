using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollExecution;

public sealed record ProcessPollExecutionsCommand(IEnumerable<SubmissionModel> Submissions)
    : ICommand<Unit>;
