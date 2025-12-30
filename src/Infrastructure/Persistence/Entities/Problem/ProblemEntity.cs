using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationCore.Domain.Problems;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("problems")]
public sealed class ProblemEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("title")]
    public required string Title { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("slug")]
    public required string Slug { get; set; }

    [Required]
    [Column("question", TypeName = "text")]
    public required string Question { get; set; }

    [Required]
    [Column("difficulty")]
    public required int Difficulty { get; set; }

    public required ICollection<TagEntity> Tags { get; set; }

    [Column("status_id")]
    public required int StatusId { get; set; } = (int)ProblemStatus.Pending;

    [Column("version")]
    public int Version { get; set; } = 1;

    [ForeignKey(nameof(StatusId))]
    public ProblemStatusEntity? Status { get; set; }

    public ICollection<ProblemSetupEntity> ProblemSetups { get; set; } = [];
}
