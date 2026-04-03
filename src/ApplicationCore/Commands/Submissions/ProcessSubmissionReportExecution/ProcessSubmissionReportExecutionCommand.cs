using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionReportExecution;

public sealed record ProcessSubmissionReportExecutionCommand(
    IEnumerable<SubmissionModel> Submissions
) : ICommand<Unit>;
