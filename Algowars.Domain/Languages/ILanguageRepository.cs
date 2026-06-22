using Algowars.Domain.Languages.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages;

public interface ILanguageRepository : IRepository<Entities.Language>
{
    Task<Entities.Language?> FindBySlugAsync(LanguageSlug slug, CancellationToken cancellationToken = default);
}
