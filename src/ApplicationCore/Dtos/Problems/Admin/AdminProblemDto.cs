using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Languages;

namespace ApplicationCore.Dtos.Problems.Admin;

public sealed record AdminProblemDto(
    Guid Id,
    string Title,
    ProblemStatus Status,
    IEnumerable<ProgrammingLanguageDto> ProgrammingLanguages,
    DateTime CreatedOn,
    AccountDto? CreatedBy
);