using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Submission.Outbox;

[Table("submission_outbox")]
public sealed class SubmissionOutboxEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Required, Column("submission_id")]
    public required Guid SubmissionId { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public SubmissionEntity? Submission { get; set; }

    [Required, Column("submission_outbox_type_id")]
    public required int SubmissionOutboxTypeId { get; set; }

    [ForeignKey(nameof(SubmissionOutboxTypeId))]
    public SubmissionOutboxTypeEntity? SubmissionOutboxType { get; set; }

    [Required, Column("submission_outbox_status_id")]
    public required int SubmissionOutboxStatusId { get; set; }

    [ForeignKey(nameof(SubmissionOutboxStatusId))]
    public SubmissionOutboxStatusEntity? SubmissionOutboxStatus { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("next_attempt_on")]
    public DateTime? NextAttemptOn { get; set; }

    [Column("last_error"), MaxLength(255)]
    public string? LastError { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("process_on")]
    public DateTime ProcessOn { get; set; }

    [Column("finalized_on")]
    public DateTime? FinalizedOn { get; set; }
}