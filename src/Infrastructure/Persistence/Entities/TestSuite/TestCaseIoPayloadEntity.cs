using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.TestSuite;

[Table("io_payloads")]
public sealed class TestCaseIoPayloadEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, Column("input")]
    public required string Input { get; set; }

    [Required, Column("expected_output")]
    public required string ExpectedOutput { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
