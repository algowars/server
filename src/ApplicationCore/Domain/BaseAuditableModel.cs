using ApplicationCore.Domain.Accounts;

namespace ApplicationCore.Domain;

public abstract class BaseAuditableModel<TId> : BaseModel<TId>
    where TId : notnull
{
    public DateTime CreatedOn { get; set; }

    public Guid? CreatedById { get; set; }

    public AccountModel? CreatedBy { get; init; }

    public DateTime? LastModifiedOn { get; set; }

    public Guid LastModifiedById { get; set; }

    public DateTime? DeletedOn { get; set; }
}