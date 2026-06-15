using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.ValueObjects;

namespace Algowars.Domain.Languages;

public interface ILanguageRepository
{
    Task AddAsync(Language language, CancellationToken cancellationToken = default);
    Task<Language?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Language?> FindBySlugAsync(LanguageSlug slug, CancellationToken cancellationToken = default);
    Task UpdateAsync(Language language, CancellationToken cancellationToken = default);
}
