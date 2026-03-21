using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_cases_expected_outputs")]
public sealed class TestCaseExpectedOutputEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Column("test_case_id")]
    public int TestCaseId { get; set; }

    [ForeignKey(nameof(TestCaseId))]
    public TestCaseEntity? TestCase { get; set; }

    [Column("value")]
    public required string Value { get; set; }

    [Column("output_type"), MaxLength(50)]
    public required string OutputType { get; set; }
}