using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Submission;

[Table("submission_statuses")]
public sealed class SubmissionStatusEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    public string? Description { get; set; }

    [Column("status_type_id")]
    public int StatusTypeId { get; set; }

    [ForeignKey(nameof(StatusTypeId))]
    public SubmissionStatusTypeEntity? StatusType { get; set; }
}