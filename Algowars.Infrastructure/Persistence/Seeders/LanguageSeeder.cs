using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders;

internal sealed class LanguageSeeder(AlgowarsDbContext context) : ISeeder
{
    // Authoritative list — add new languages/versions here.
    // Versions not present in this list will be deprecated automatically.
    private static readonly DesiredLanguage[] DesiredLanguages =
    [
        new("JavaScript", "javascript",
        [
            new(new LanguageVersion("Node.js 22.08.0"), new Judge0Id(102)),
        ]),
        new("TypeScript", "typescript",
        [
            new(new LanguageVersion("5.6.2"), new Judge0Id(101)),
        ]),
        new("Python", "python",
        [
            new(new LanguageVersion("3.13.2"), new Judge0Id(109)),
        ]),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        List<Language> existingLanguages = await context.Languages
            .IgnoreQueryFilters()
            .Include(l => l.Versions)
            .ToListAsync(cancellationToken);

        foreach (DesiredLanguage desired in DesiredLanguages)
        {
            Language? language = existingLanguages
                .FirstOrDefault(l => l.Slug == new LanguageSlug(desired.Slug));

            if (language is null)
            {
                language = new Language(new LanguageName(desired.Name), new LanguageSlug(desired.Slug));
                context.Languages.Add(language);
            }
            else if (!language.IsActive)
            {
                language.Activate();
            }

            HashSet<int> desiredJudge0Ids = desired.Versions
                .Select(v => v.Judge0Id.Value)
                .ToHashSet();

            // Deprecate versions no longer in the desired list
            foreach (LanguageVersionEntry existing in language.Versions)
            {
                if (!desiredJudge0Ids.Contains(existing.Judge0Id.Value))
                    existing.Deprecate();
                else if (!existing.IsActive)
                    existing.Activate();
            }

            // Add versions that don't exist yet
            HashSet<int> existingJudge0Ids = language.Versions
                .Select(v => v.Judge0Id.Value)
                .ToHashSet();

            foreach (DesiredVersion version in desired.Versions)
            {
                if (!existingJudge0Ids.Contains(version.Judge0Id.Value))
                    language.AddVersion(version.Version, version.Judge0Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed record DesiredLanguage(string Name, string Slug, DesiredVersion[] Versions);
    private sealed record DesiredVersion(LanguageVersion Version, Judge0Id Judge0Id);
}
