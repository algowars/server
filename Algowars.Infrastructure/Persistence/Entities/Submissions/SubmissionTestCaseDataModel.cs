using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Submissions;

[Table("submission_test_cases")]
public sealed class SubmissionTestCaseDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public SubmissionDataModel? Submission { get; set; }

    [Column("test_case_id")]
    public Guid TestCaseId { get; set; }
}
