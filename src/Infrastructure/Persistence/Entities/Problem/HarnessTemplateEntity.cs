using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("harness_templates")]
public sealed class HarnessTemplateEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, Column("template")]
    public required string Template { get; set; }

    public IEnumerable<ProblemSetupEntity> ProblemSetups { get; set; } = [];
}
