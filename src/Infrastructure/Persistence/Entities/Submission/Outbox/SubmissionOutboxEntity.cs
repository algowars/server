using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Submission.Outbox;

[Table("submission_outbox")]
public sealed class SubmissionOutboxEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public SubmissionEntity? Submission { get; set; }

    [Column("submission_outbox_type_id")]
    public int TypeId { get; set; }

    [ForeignKey(nameof(TypeId))]
    public SubmissionOutboxTypeEntity? Type { get; set; }

    [Column("submission_outbox_status_id")]
    public int StatusId { get; set; }

    [ForeignKey(nameof(StatusId))]
    public SubmissionOutboxStatusEntity? Status { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("next_attempt_on")]
    public DateTime? NextAttemptOn { get; set; }

    [Column("last_error")]
    public string? LastErrror { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("process_on")]
    public DateTime? ProcessedOn { get; set; }
}
