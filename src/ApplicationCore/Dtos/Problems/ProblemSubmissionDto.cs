using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Dtos.Problems;

public sealed record ProblemSubmissionDto(
    AccountDto CreatedBy,
    string Code,
    string Status,
    string Language,
    string LanguageVersion,
    DateTime CreatedOn,
    int RuntimeMs,
    int MemoryKb
);
