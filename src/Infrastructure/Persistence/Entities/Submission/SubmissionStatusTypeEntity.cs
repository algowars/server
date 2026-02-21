using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Submission;

[Table("submission_status_types")]
public sealed class SubmissionStatusTypeEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    public string? Description { get; set; }
}