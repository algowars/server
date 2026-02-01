using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_cases")]
public sealed class TestCaseEntity : BaseAuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("test_suite_id")]
    public int TestSuiteId { get; set; }

    [Column("test_case_type_id")]
    public int TestCaseTypeId { get; set; }

    [ForeignKey(nameof(TestCaseTypeId))]
    public TestCaseTypeEntity? TestCaseType { get; set; }

    [ForeignKey(nameof(TestSuiteId))]
    public TestSuiteEntity? TestSuite { get; set; }

    public IEnumerable<TestCaseInputParamEntity> InputParams { get; set; } = [];
}
