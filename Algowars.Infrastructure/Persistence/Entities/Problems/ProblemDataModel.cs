using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Problems;

[Table("problems")]
public sealed class ProblemDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(200), Column("slug")]
    public required string Slug { get; set; }

    [Column("status")]
    public int Status { get; set; }

    public ICollection<ProblemVersionDataModel> Versions { get; set; } = [];
}
