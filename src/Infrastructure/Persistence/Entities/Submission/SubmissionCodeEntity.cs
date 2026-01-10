using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Infrastructure.Persistence.Entities.Submission;

[Table("submission_codes")]
public sealed class SubmissionCodeEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Required, Column("code", TypeName = "TEXT")]
    public required string Code { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
