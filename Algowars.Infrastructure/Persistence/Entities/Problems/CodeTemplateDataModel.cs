using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Problems;

[Table("code_templates")]
public sealed class CodeTemplateDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("problem_version_id")]
    public Guid ProblemVersionId { get; set; }

    [ForeignKey(nameof(ProblemVersionId))]
    public ProblemVersionDataModel? ProblemVersion { get; set; }

    [Column("language_version_id")]
    public Guid LanguageVersionId { get; set; }

    [Required, Column("starter_code")]
    public required string StarterCode { get; set; }

    [Required, Column("wrapper_code")]
    public required string WrapperCode { get; set; }
}
