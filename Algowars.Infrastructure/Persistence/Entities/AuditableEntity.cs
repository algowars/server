using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities;

public abstract class AuditableEntity
{
    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Column("updated_on")]
    public DateTime? UpdatedOn { get; set; }
}
