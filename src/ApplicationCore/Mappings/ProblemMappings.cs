using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems;
using Mapster;

namespace ApplicationCore.Mappings;

public sealed class ProblemMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<ProblemModel, ProblemDto>()
            .Map(d => d.AvailableLanguages, s => s.GetAvailableLanguages());

        config.NewConfig<ProgrammingLanguage, ProgrammingLanguageDto>();

        config.NewConfig<LanguageVersion, LanguageVersionDto>();
    }
}