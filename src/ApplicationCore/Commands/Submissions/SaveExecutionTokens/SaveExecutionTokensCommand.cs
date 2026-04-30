using ApplicationCore.Domain.Submissions;
using MediatR;

namespace ApplicationCore.Commands.Submissions.SaveExecutionTokens;

public sealed record SaveExecutionTokensCommand(IEnumerable<SubmissionModel> Submissions)
    : ICommand<Unit>;