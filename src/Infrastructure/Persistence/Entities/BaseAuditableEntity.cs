using Infrastructure.Persistence.Entities.Account;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities;

public abstract class BaseAuditableEntity
{
    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("created_by_id")]
    public Guid? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public AccountEntity? CreatedBy { get; set; }

    [Column("last_modified_on")]
    public DateTime? LastModifiedOn { get; set; }

    [Column("last_modified_by_id")]
    public Guid? LastModifiedById { get; set; }

    [ForeignKey(nameof(LastModifiedById))]
    public AccountEntity? LastModifiedByAccount { get; set; }

    [Column("deleted_on")]
    public DateTime? DeletedOn { get; set; }
}