using ApplicationCore.Domain.Accounts;

namespace UnitTests.ApplicationCore.Domain.Accounts;

[TestFixture]
public sealed class AccountTests
{
    [Test]
    public void Creating_account_with_username_sets_value()
    {
        var account = new Account
        {
            Username = "user1"
        };

        Assert.That(account.Username, Is.EqualTo("user1"));
    }

    [Test]
    public void Optional_properties_can_be_set()
    {
        const string sub = "auth0|123";
        const string imageUrl = "https://example.com/avatar.png";
        var createdOn = DateTime.UtcNow;

        var account = new Account
        {
            Username = "user1",
            Sub = sub,
            ImageUrl = imageUrl,
            CreatedOn = createdOn
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(account.Sub, Is.EqualTo(sub));
            Assert.That(account.ImageUrl, Is.EqualTo(imageUrl));
            Assert.That(account.CreatedOn, Is.EqualTo(createdOn));
        }
    }

    [Test]
    public void Last_modified_fields_default_to_null()
    {
        var account = new Account
        {
            Username = "user1"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(account.LastModifiedOn, Is.Null);
            Assert.That(account.LastModifiedById, Is.Null);
        }
    }

    [Test]
    public void Last_modified_fields_can_be_updated()
    {
        var modifiedOn = DateTime.UtcNow;
        var modifiedBy = Guid.NewGuid();

        var account = new Account
        {
            Username = "user1"
        };

        account.LastModifiedOn = modifiedOn;
        account.LastModifiedById = modifiedBy;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(account.LastModifiedOn, Is.EqualTo(modifiedOn));
            Assert.That(account.LastModifiedById, Is.EqualTo(modifiedBy));
        }
    }
}