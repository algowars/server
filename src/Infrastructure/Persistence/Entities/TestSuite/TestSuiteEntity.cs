using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Entities.Problem;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_suites")]
public sealed class TestSuiteEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, Column("name")]
    public required string Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("test_suite_type_id")]
    public int TestSuiteTypeId { get; set; }

    [ForeignKey(nameof(TestSuiteTypeId))]
    public TestSuiteTypeEntity? TestSuiteType { get; set; }

    public IEnumerable<TestCaseEntity> TestCases { get; set; } = [];

    public IEnumerable<ProblemSetupEntity> Setups { get; set; } = [];
}
