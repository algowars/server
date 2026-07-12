using Algowars.Domain.Submissions.Enums;

namespace Algowars.Application.Submissions.Dtos;

public sealed record CreateSubmissionDto(
    Guid ProblemSetupId,
    SubmissionType Type,
    string Code,
    Guid CreatedById);
