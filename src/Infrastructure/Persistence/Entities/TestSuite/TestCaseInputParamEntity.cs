using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table(("test_case_input_params"))]
public sealed class TestCaseInputParamEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, Column("order")]
    public int Order { get; set; }

    [Required, Column("name")]
    public required string Name { get; set; }

    [Required, Column("test_case_input_type_id")]
    public int TestCaseInputTypeId { get; set; }

    [ForeignKey(nameof(TestCaseInputTypeId))]
    public required TestCaseInputParamTypeEntity Type { get; set; }

    [Required, Column("test_case_id")]
    public int TestCaseId { get; set; }
}
