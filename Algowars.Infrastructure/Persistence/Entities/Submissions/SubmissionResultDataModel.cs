using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Submissions;

[Table("submission_results")]
public sealed class SubmissionResultDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public SubmissionDataModel? Submission { get; set; }

    [Column("test_case_id")]
    public Guid TestCaseId { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("runtime_ms")]
    public int? RuntimeMs { get; set; }

    [Column("memory_kb")]
    public int? MemoryKb { get; set; }

    [MaxLength(4096), Column("actual_output")]
    public string? ActualOutput { get; set; }

    [Column("stderr")]
    public string? Stderr { get; set; }

    [Column("compile_output")]
    public string? CompileOutput { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
