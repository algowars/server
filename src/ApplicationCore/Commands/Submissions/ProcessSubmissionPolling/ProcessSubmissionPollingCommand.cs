using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionPolling;

public sealed record ProcessSubmissionPollingCommand(IEnumerable<SubmissionModel> Submissions)
    : ICommand<Unit>;
