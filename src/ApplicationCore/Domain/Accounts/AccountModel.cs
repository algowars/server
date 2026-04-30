namespace ApplicationCore.Domain.Accounts;

public sealed class AccountModel : BaseModel<Guid>
{
    private string _username = string.Empty;

    public string? Sub { get; init; }

    public required string Username
    {
        get => _username;
        init => _username = value;
    }

    public string? PreviousUsername { get; private set; }

    public DateTime? UsernameLastChangedAt { get; private set; }

    public string? ImageUrl { get; init; }

    public DateTime CreatedOn { get; init; }

    public DateTime? LastModifiedOn { get; set; }

    public Guid? LastModifiedById { get; set; }

    public void ChangeUsername(string newUsername)
    {
        if (Username == newUsername)
        {
            return;
        }

        PreviousUsername = Username;
        _username = newUsername;
        UsernameLastChangedAt = DateTime.UtcNow;
    }
}