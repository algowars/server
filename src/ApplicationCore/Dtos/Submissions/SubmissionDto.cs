using ApplicationCore.Domain.Submissions;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Languages;

namespace ApplicationCore.Dtos.Submissions;

public sealed record SubmissionDto(
    Guid Id,
    string Code,
    LanguageVersionDto LanguageVersion,
    AccountDto CreatedBy,
    DateTime CreatedOn,
    SubmissionStatus Status
);
