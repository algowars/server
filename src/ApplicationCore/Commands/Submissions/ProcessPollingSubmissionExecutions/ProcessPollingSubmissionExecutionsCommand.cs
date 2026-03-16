using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollingSubmissionExecutions;

public sealed record ProcessPollingSubmissionExecutionsCommand(IEnumerable<SubmissionModel> Submissions) : ICommand<Unit>;