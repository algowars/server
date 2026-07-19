using Algowars.Domain.Submissions.Enums;

namespace Algowars.Application.Submissions.Dtos;

public sealed record SubmissionResultStatusDto(
    SubmissionResultStatus Status,
    int? Runtime,
    int? MemoryUsed,
    string? ActualOutput,
    string? ExpectedOutput,
    string? StandardOutput,
    string? StandardError,
    string? CompileOutput);

public sealed record SubmissionStatusDto(
    Guid SubmissionId,
    Guid ProblemSetupId,
    SubmissionStatus Status,
    IReadOnlyCollection<SubmissionResultStatusDto> Results);