using ApplicationCore.Domain;

namespace UnitTests.ApplicationCore.Domain;

[TestFixture]
public sealed class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity<Guid> { }

    [Test]
    public void Id_can_be_set()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id };

        Assert.That(entity.Id, Is.EqualTo(id));
    }

    [Test]
    public void Id_can_be_updated()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        var entity = new TestEntity { Id = firstId };

        entity.Id = secondId;

        Assert.That(entity.Id, Is.EqualTo(secondId));
    }
}
