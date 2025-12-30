using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("harness_templates")]
public sealed class HarnessTemplateEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template")]
    public string Template { get; set; }

    public ICollection<ProblemSetupEntity> ProblemSetups { get; set; }
}
