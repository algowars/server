using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Problem;

[Table("problem_history")]
public sealed class ProblemHistoryEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("problem_id")]
    public Guid ProblemId { get; set; }

    [Column("version")]
    public int Version { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("slug")]
    public string Slug { get; set; }

    [Column("difficulty")]
    public int Difficulty { get; set; }

    [Column("question")]
    public string Question { get; set; }

    [Column("archived_on")]
    public DateTime ArchivedOn { get; set; }

    [Column("archived_by_id")]
    public Guid? ArchivedById { get; set; }

    [Column("archive_reason")]
    public string? ArchiveReason { get; set; }
}
