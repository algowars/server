using Algowars.Domain.Submissions.Enums;

namespace Algowars.Application.Submissions.Dtos;

public sealed record SubmissionUserDto(
    Guid Id,
    string Username,
    string? ImageUrl);

public sealed record ProblemSubmissionDto(
    Guid Id,
    SubmissionType Type,
    SubmissionStatus Status,
    DateTime CreatedAt,
    SubmissionUserDto User);
