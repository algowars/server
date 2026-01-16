using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Submission.Outbox;

[Table("submission_outbox_types")]
public sealed class SubmissionOutboxTypeEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500), Column("description")]
    public string? Description { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
