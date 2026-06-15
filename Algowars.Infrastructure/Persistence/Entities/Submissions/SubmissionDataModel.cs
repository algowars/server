using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Submissions;

[Table("submissions")]
public sealed class SubmissionDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("problem_version_id")]
    public Guid ProblemVersionId { get; set; }

    [Column("language_version_id")]
    public Guid LanguageVersionId { get; set; }

    [Column("type")]
    public int Type { get; set; }

    [Required, Column("source_code")]
    public required string SourceCode { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<SubmissionResultDataModel> Results { get; set; } = [];
    public ICollection<SubmissionTestCaseDataModel> TestCases { get; set; } = [];
}
