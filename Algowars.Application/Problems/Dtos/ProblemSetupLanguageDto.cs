
namespace Algowars.Application.Problems.Dtos;

public sealed record ProblemSetupLanguageVersionDto(int LanguageVersionId, string LanguageVersionName);

public sealed record ProblemSetupLanguageDto(int LanguageId, string LanguageName, IEnumerable<ProblemSetupLanguageVersionDto> LanguageVersions);