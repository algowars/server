using ApplicationCore.Dtos.Languages;

namespace ApplicationCore.Queries.Problems.GetAvailableLanguages;

public sealed record GetAvailableLanguagesQuery() : IQuery<IEnumerable<ProgrammingLanguageDto>>;