using Algowars.Domain.Submissions.Enums;
using MediatR;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

using Algowars.Application.Submissions.Dtos;

internal sealed record CreateSubmissionCommand(
    Guid ProblemSetupId,
    SubmissionType Type,
    string Code,
    Guid CreatedById,
    IReadOnlyCollection<CreateSubmissionCustomTestCaseDto>? CustomTestCases) : ICommand<Guid>;
