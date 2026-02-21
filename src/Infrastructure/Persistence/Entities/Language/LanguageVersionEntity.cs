using Infrastructure.Persistence.Entities.Problem;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Language;

[Table("programming_language_versions")]
public sealed class LanguageVersionEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; init; }

    [Required]
    [MaxLength(20)]
    [Column("version")]
    public required string Version { get; init; }

    [Column("programming_language_id")]
    public int ProgrammingLanguageId { get; init; }

    [Column("initial_code")]
    public string? InitialCode { get; init; }

    [ForeignKey(nameof(ProgrammingLanguageId))]
    public ProgrammingLanguageEntity? ProgrammingLanguage { get; set; }

    public IEnumerable<ProblemSetupEntity> ProblemSetups { get; set; } = [];
}