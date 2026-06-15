using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Problems;

[Table("problem_versions")]
public sealed class ProblemVersionDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("problem_id")]
    public Guid ProblemId { get; set; }

    [ForeignKey(nameof(ProblemId))]
    public ProblemDataModel? Problem { get; set; }

    [Required, MaxLength(200), Column("title")]
    public required string Title { get; set; }

    [Required, Column("question")]
    public required string Question { get; set; }

    [Column("difficulty")]
    public int Difficulty { get; set; }

    [Column("time_limit_ms")]
    public int TimeLimitMs { get; set; }

    [Column("memory_limit_kb")]
    public int MemoryLimitKb { get; set; }

    [Column("version_number")]
    public int VersionNumber { get; set; }

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    public ICollection<TestCaseDataModel> TestCases { get; set; } = [];
    public ICollection<CodeTemplateDataModel> CodeTemplates { get; set; } = [];
}
