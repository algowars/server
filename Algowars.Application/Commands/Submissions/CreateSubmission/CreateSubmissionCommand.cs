using Algowars.Domain.Submissions.Enums;
using MediatR;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

internal sealed record CreateSubmissionCommand(
    Guid ProblemSetupId,
    SubmissionType Type,
    string Code,
    Guid CreatedById) : ICommand<Unit>;