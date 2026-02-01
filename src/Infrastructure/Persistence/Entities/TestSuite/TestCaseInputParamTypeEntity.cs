using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Index(nameof(Name), IsUnique = true)]
[Table("test_case_input_param_types")]
public sealed class TestCaseInputParamTypeEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, Column("name")]
    public string Name { get; set; } = "";
}
