
namespace Algowars.Application.Problems.Dtos;

public sealed record ProblemSetupLanguageVersionDto(Guid VersionId, string Version);

public sealed record ProblemSetupLanguageDto(Guid Id, string Name, IEnumerable<ProblemSetupLanguageVersionDto> Versions);