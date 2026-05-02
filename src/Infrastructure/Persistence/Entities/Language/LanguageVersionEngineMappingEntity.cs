using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Language;

[Table("language_version_engine_mappings")]
public sealed class LanguageVersionEngineMappingEntity
{
    [Key, Column("id")]
    public int Id { get; init; }

    [Column("programming_language_version_id")]
    public int ProgrammingLanguageVersionId { get; init; }

    [ForeignKey(nameof(ProgrammingLanguageVersionId))]
    public LanguageVersionEntity? LanguageVersion { get; init; }

    [Column("engine_id")]
    public int EngineId { get; init; }

    [Column("engine_language_id")]
    public int EngineLanguageId { get; init; }

    [Column("engine_language_name")]
    public string? EngineLanguageName { get; init; }
}
