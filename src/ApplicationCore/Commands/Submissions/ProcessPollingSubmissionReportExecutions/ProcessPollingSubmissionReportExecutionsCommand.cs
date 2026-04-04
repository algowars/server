using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollingSubmissionReportExecutions;

public sealed record ProcessPollingSubmissionReportExecutionsCommand(IEnumerable<SubmissionModel> Submissions) : ICommand<Unit>;
