using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Infrastructure.Persistence.Entities.Submission.Outbox;

[Table("submission_outbox_statuses")]
public sealed class SubmissionOutboxStatusEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, Column("name"), MaxLength(100)]
    public required string Name { get; set; }

    [Column("description"), MaxLength(500)]
    public string? Description { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
