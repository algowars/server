using ApplicationCore.Dtos.Languages;

namespace ApplicationCore.Queries.Accounts.GetAvailableLanguages;

public sealed record GetAvailableLanguagesQuery() : IQuery<IEnumerable<ProgrammingLanguageDto>>;
