using Algowars.Application.Languages;
using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.Enums;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class LanguageReadRepository(AlgowarsDbContext context) : ILanguageReadRepository
{
    public async Task<IEnumerable<Language>> FindLanguagesByVersionId(IEnumerable<Guid> versionIds, CancellationToken cancellationToken)
    => await context.Languages
        .AsNoTracking()
        .Include(l => l.Versions)
        .Where(l => l.Versions.Any(v => versionIds.Contains(v.Id)))
        .ToListAsync(cancellationToken);
}