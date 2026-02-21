namespace ApplicationCore.Domain.Accounts;

public sealed class AccountModel : BaseModel<Guid>
{
    public string? Sub { get; init; }

    public required string Username { get; init; }

    public string? ImageUrl { get; init; }

    public DateTime CreatedOn { get; init; }

    public DateTime? LastModifiedOn { get; set; }

    public Guid? LastModifiedById { get; set; }
}