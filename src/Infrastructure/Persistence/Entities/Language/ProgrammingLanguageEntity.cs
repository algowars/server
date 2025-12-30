using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Language;

[Table("programming_languages")]
public sealed class ProgrammingLanguageEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; init; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public required string Name { get; init; }

    [Column("is_archived")]
    public bool IsArchived { get; init; }

    public required ICollection<LanguageVersionEntity> Versions { get; set; }
}
