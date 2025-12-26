using ApplicationCore.Dtos.Languages;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Problems.GetAvailableLanguages;

public sealed class GetAvailableLanguagesHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetAvailableLanguagesQuery, IEnumerable<ProgrammingLanguageDto>>
{
    public async Task<Result<IEnumerable<ProgrammingLanguageDto>>> Handle(
        GetAvailableLanguagesQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var languages = await problemRepository.GetAllLanguagesAsync(cancellationToken);

            var dtos = languages.Select(language => new ProgrammingLanguageDto
            {
                Id = language.Id,
                Name = language.Name,
                Versions =
                [
                    .. language.Versions.Select(version => new LanguageVersionDto
                    {
                        Id = version.Id,
                        Version = version.Version,
                        InitialCode = version.InitialCode,
                    }),
                ],
            });

            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
