using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Submissions;

[Table("submission_outbox_steps")]
public sealed class SubmissionOutboxDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [Column("step")]
    public int Step { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("max_attempts")]
    public int MaxAttempts { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_attempt_at")]
    public DateTime? LastAttemptAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [MaxLength(2000), Column("last_error")]
    public string? LastError { get; set; }
}
