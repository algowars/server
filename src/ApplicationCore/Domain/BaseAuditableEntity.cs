using ApplicationCore.Domain.Accounts;

namespace ApplicationCore.Domain;


public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>
    where TId : notnull
{
    public DateTime CreatedOn { get; set; }
    
    public Guid? CreatedById { get; set; }
    
    public Account? CreatedBy { get; init; }
    
    public DateTime? LastModifiedOn { get; set; }
    
    public Guid LastModifiedById { get; set; }
    
    public DateTime? DeletedOn { get; set; }
}