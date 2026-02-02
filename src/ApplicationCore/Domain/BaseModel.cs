namespace ApplicationCore.Domain;

public abstract class BaseModel<TId>
    where TId : notnull
{
    public TId? Id { get; set; }
}