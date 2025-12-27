using ApplicationCore.Domain;
using ApplicationCore.Domain.Accounts;

namespace UnitTests.ApplicationCore.Domain;

[TestFixture]
public sealed class BaseAuditableModelTests
{
    private sealed class TestAuditableModel : BaseAuditableModel<Guid> { }

    [Test]
    public void All_properties_are_null_or_default_by_default()
    {
        var entity = new TestAuditableModel();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.CreatedOn, Is.EqualTo(default(DateTime)));
            Assert.That(entity.CreatedById, Is.Null);
            Assert.That(entity.CreatedBy, Is.Null);
            Assert.That(entity.LastModifiedOn, Is.Null);
            Assert.That(entity.LastModifiedById, Is.EqualTo(default(Guid)));
            Assert.That(entity.DeletedOn, Is.Null);
        }
    }

    [Test]
    public void Created_properties_can_be_set()
    {
        var createdOn = DateTime.UtcNow;
        var createdById = Guid.NewGuid();
        var account = new AccountModel { Username = "creator" };

        var entity = new TestAuditableModel
        {
            CreatedOn = createdOn,
            CreatedById = createdById,
            CreatedBy = account,
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.CreatedOn, Is.EqualTo(createdOn));
            Assert.That(entity.CreatedById, Is.EqualTo(createdById));
            Assert.That(entity.CreatedBy, Is.EqualTo(account));
        }
    }

    [Test]
    public void Last_modified_properties_can_be_set()
    {
        var modifiedOn = DateTime.UtcNow;
        var modifiedById = Guid.NewGuid();

        var entity = new TestAuditableModel
        {
            LastModifiedOn = modifiedOn,
            LastModifiedById = modifiedById,
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.LastModifiedOn, Is.EqualTo(modifiedOn));
            Assert.That(entity.LastModifiedById, Is.EqualTo(modifiedById));
        }
    }

    [Test]
    public void Deleted_on_can_be_set()
    {
        var deletedOn = DateTime.UtcNow;

        var entity = new TestAuditableModel { DeletedOn = deletedOn };

        Assert.That(entity.DeletedOn, Is.EqualTo(deletedOn));
    }
}
