using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("problem_statuses")]
public sealed class ProblemStatusEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; init; }

    [Column("description")]
    [MaxLength(100)]
    public string? Description { get; init; }

    public ICollection<ProblemEntity>? Problems { get; set; }
}
