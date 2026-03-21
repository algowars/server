using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_cases_inputs")]
public sealed class TestCaseInputEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Column("test_case_id")]
    public int TestCaseId { get; set; }

    [ForeignKey(nameof(TestCaseId))]
    public TestCaseEntity? TestCase { get; set; }

    [Column("value")]
    public required string Value { get; set; }

    [Column("test_cases_inputs_value_type_id")]
    public int TestCasesInputsValueTypeId { get; set; }

    [ForeignKey(nameof(TestCasesInputsValueTypeId))]
    public TestCasesInputsValueTypeEntity? TestCasesInputsValueType { get; set; }
}