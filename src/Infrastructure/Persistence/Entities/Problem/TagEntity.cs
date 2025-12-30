using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("tags")]
public sealed class TagEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [MaxLength(50)]
    [Column("value")]
    public required string Value { get; set; }

    public ICollection<ProblemEntity>? Problems { get; set; }
}
