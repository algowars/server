using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Languages;

[Table("programming_language_versions")]
public sealed class LanguageVersionDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(20), Column("version")]
    public required string Version { get; set; }

    [Column("programming_language_id")]
    public Guid ProgrammingLanguageId { get; set; }

    [ForeignKey(nameof(ProgrammingLanguageId))]
    public ProgrammingLanguageDataModel? ProgrammingLanguage { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}
