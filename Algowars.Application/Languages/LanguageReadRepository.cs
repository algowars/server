using Algowars.Domain.Languages.Entities;

namespace Algowars.Application.Languages;

public interface ILanguageReadRepository
{
    Task<IEnumerable<Language>> FindLanguagesByVersionId(IEnumerable<Guid> versionIds, CancellationToken cancellationToken);
}
