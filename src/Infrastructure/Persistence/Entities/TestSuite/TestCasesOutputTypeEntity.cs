using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("test_cases_output_types")]
public sealed class TestCasesOutputTypeEntity
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Column("name"), MaxLength(50)]
    public required string Name { get; set; }
}