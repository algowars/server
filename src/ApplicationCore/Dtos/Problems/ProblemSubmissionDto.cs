using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Dtos.Problems;

public sealed class ProblemSubmissionDto(
    AccountDto CreatedBy,
    string Code,
    string Status,
    string Language,
    string LanguageVersion,
    DateTime CreatedOn,
    int RuntimeMs,
    int MemoryKb
);
