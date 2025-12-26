using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Domain.Problems;

public sealed class ProblemModel : BaseAuditableModel<Guid>
{
    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Question { get; init; }

    public required IEnumerable<TagModel> Tags { get; init; }

    public int Difficulty { get; init; }

    public ProblemStatus Status { get; init; }

    public ICollection<ProblemSetup> ProblemSetups { get; init; } = [];

    public int Version { get; init; }

    public IEnumerable<ProgrammingLanguage> GetAvailableLanguages()
    {
        if (ProblemSetups.Count == 0)
        {
            return [];
        }

        return ProblemSetups
            .Where(s => s.LanguageVersion?.ProgrammingLanguage != null)
            .GroupBy(s => s.LanguageVersion!.ProgrammingLanguage!)
            .Select(g =>
            {
                var language = g.Key;

                var versions = g.Select(s => s.LanguageVersion!)
                    .GroupBy(v => v.Id)
                    .Select(vg => vg.First())
                    .OrderBy(v => v.Version)
                    .ToList();

                return new ProgrammingLanguage
                {
                    Id = language.Id,
                    Name = language.Name,
                    IsArchived = language.IsArchived,
                    Versions = versions,
                };
            })
            .DistinctBy(s => s.Id)
            .ToList();
    }
}
