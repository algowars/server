using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Problems;

[Table("test_cases")]
public sealed class TestCaseDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("problem_version_id")]
    public Guid ProblemVersionId { get; set; }

    [ForeignKey(nameof(ProblemVersionId))]
    public ProblemVersionDataModel? ProblemVersion { get; set; }

    [Required, Column("input")]
    public required string Input { get; set; }

    [Required, Column("expected_output")]
    public required string ExpectedOutput { get; set; }

    [Column("is_hidden")]
    public bool IsHidden { get; set; }
}
