using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_cases_inputs_value_types")]
public sealed class TestCasesInputsValueTypeEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Column("name"), MaxLength(50)]
    public required string Name { get; set; }
}