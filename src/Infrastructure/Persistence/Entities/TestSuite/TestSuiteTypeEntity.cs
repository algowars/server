using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_suite_types")]
public sealed class TestSuiteTypeEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name"), MaxLength(50)]
    public required string Name { get; set; }

    [Column("description"), MaxLength(100)]
    public string? Description { get; set; }
}