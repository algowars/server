using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Entities.Language;
using Infrastructure.Persistence.Entities.TestSuite;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("problem_setups")]
public sealed class ProblemSetupEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("problem_id")]
    public Guid ProblemId { get; set; }

    [ForeignKey(nameof(ProblemId))]
    public ProblemEntity? Problem { get; set; }

    [Column("programming_language_version_id")]
    public int ProgrammingLanguageVersionId { get; set; }

    [ForeignKey(nameof(ProgrammingLanguageVersionId))]
    public LanguageVersionEntity? LanguageVersion { get; set; }

    [Column("version")]
    public int Version { get; set; }

    [Column("initial_code")]
    public string? InitialCode { get; set; }

    [Column("function_name")]
    public string? FunctionName { get; set; }

    [Column("harness_template_id")]
    public int HarnessTemplateId { get; set; }

    [ForeignKey(nameof(HarnessTemplateId))]
    public HarnessTemplateEntity? HarnessTemplate { get; set; }

    public IEnumerable<TestSuiteEntity> TestSuites { get; set; } = [];
}
