using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Languages;

[Table("programming_languages")]
public sealed class ProgrammingLanguageDataModel
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(50), Column("name")]
    public required string Name { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    public ICollection<LanguageVersionDataModel> Versions { get; set; } = [];
}
