using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_cases")]
public sealed class TestCaseEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Column("test_suite_id")]
    public int TestSuiteId { get; set; }

    [Column("name"), MaxLength(100)]
    public string? Name { get; set; }

    [Column("description"), MaxLength(200)]
    public string? Description { get; set; }

    [ForeignKey(nameof(TestSuiteId))]
    public TestSuiteEntity? TestSuite { get; set; }

    public IEnumerable<TestCaseInputEntity> InputParams { get; set; } = [];

    public TestCaseExpectedOutputEntity? ExpectedOutput { get; set; }
}