using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Submission;

[Table("submission_results")]
public sealed class SubmissionResultEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [ForeignKey(nameof(StatusId))]
    public required SubmissionStatusEntity Status { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }

    [Column("stdout")]
    public string? Stdout { get; set; }

    [Column("stderr")]
    public string? Stderr { get; set; }

    [Column("runtime_ms")]
    public int? RuntimeMs { get; set; }

    [Column("memory_kb")]
    public int? MemoryKb { get; set; }
}
