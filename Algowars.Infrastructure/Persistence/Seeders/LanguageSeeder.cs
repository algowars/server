using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders;

internal sealed class LanguageSeeder(AlgowarsDbContext context) : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Languages.AnyAsync(cancellationToken))
            return;

        Language javascript = new(new LanguageName("JavaScript"), new LanguageSlug("javascript"));
        javascript.AddVersion(new LanguageVersion("Node.js 22.08.0"), new Judge0Id(102));

        Language typescript = new(new LanguageName("TypeScript"), new LanguageSlug("typescript"));
        typescript.AddVersion(new LanguageVersion("5.6.2"), new Judge0Id(101));

        Language python = new(new LanguageName("Python"), new LanguageSlug("python"));
        python.AddVersion(new LanguageVersion("3.13.2"), new Judge0Id(109));

        context.Languages.AddRange(javascript, typescript, python);
        await context.SaveChangesAsync(cancellationToken);
    }
}
