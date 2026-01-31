using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Entities.Account;
using Infrastructure.Persistence.Entities.Problem;

namespace Infrastructure.Persistence.Entities.Submission;

[Table("submissions")]
public sealed class SubmissionEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("code")]
    public required string Code { get; set; }

    [Required, Column("problem_setup_id")]
    public required int ProblemSetupId { get; set; }

    [ForeignKey(nameof(ProblemSetupId))]
    public ProblemSetupEntity? ProblemSetup { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Required, Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public AccountEntity? CreatedBy { get; set; }

    public List<SubmissionResultEntity> Results { get; set; } = [];
}
