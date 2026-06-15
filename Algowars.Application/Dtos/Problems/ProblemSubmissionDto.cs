using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Dtos.Problems;

public sealed record ProblemSubmissionDto(
    UserDto? CreatedBy,
    string Code,
    string Status,
    string Language,
    string LanguageVersion,
    DateTime CreatedOn,
    double? RuntimeMs,
    double? MemoryKb);
