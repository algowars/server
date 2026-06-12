using Algowars.Domain.Languages.ValueObjects;

namespace Algowars.Domain.Languages;

public interface ILanguageRepository
{
    Task AddAsync(Entities.Language language, CancellationToken cancellationToken = default);
    Task<Entities.Language?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Language?> FindBySlugAsync(LanguageSlug slug, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entities.Language language, CancellationToken cancellationToken = default);
}
