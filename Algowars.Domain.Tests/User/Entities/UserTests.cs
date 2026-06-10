using Algowars.Domain.User.Exceptions;
using Algowars.Domain.User.ValueObjects;
using UserEntity = Algowars.Domain.User.Entities.User;

namespace Algowars.Domain.Tests.User.Entities;

public class UserTests
{
    private static readonly Username ValidUsername = new("alice");
    private const string ValidSub = "auth0|abc123";

    [Test]
    public void Constructor_ValidArguments_CreatesUser()
    {
        var user = new UserEntity(ValidUsername, ValidSub);

        Assert.Multiple(() =>
        {
            Assert.That(user.Username, Is.EqualTo(ValidUsername));
            Assert.That(user.Sub, Is.EqualTo(ValidSub));
            Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public void Constructor_NullUsername_ThrowsInvalidUsernameException()
    {
        Assert.Throws<InvalidUsernameException>(() => new UserEntity(null!, ValidSub));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    public void Constructor_EmptyOrWhitespaceSub_ThrowsInvalidUserSubException(string sub)
    {
        Assert.Throws<InvalidUserSubException>(() => new UserEntity(ValidUsername, sub));
    }

    [Test]
    public void Constructor_SetsSubCorrectly()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        Assert.That(user.Sub, Is.EqualTo(ValidSub));
    }

    [Test]
    public void Constructor_SubWithSpecialCharacters_Succeeds()
    {
        var user = new UserEntity(ValidUsername, "google-oauth2|abc.123-xyz");
        Assert.That(user.Sub, Is.EqualTo("google-oauth2|abc.123-xyz"));
    }

    [Test]
    public void Constructor_GeneratesNonEmptyId()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Constructor_GeneratesUniqueIds()
    {
        var user1 = new UserEntity(ValidUsername, ValidSub);
        var user2 = new UserEntity(ValidUsername, ValidSub);

        Assert.That(user1.Id, Is.Not.EqualTo(user2.Id));
    }

    [Test]
    public void Equals_SameInstance_IsEqual()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        Assert.That(user, Is.EqualTo(user));
    }

    [Test]
    public void Equals_DifferentInstances_AreNotEqual()
    {
        var user1 = new UserEntity(ValidUsername, ValidSub);
        var user2 = new UserEntity(ValidUsername, ValidSub);

        Assert.That(user1, Is.Not.EqualTo(user2));
    }

    [Test]
    public void Equals_Null_IsNotEqual()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        Assert.That(user.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_SameUser_ReturnsSameHash()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        Assert.That(user.GetHashCode(), Is.EqualTo(user.GetHashCode()));
    }

    [Test]
    public void GetHashCode_DifferentUsers_ReturnDifferentHashes()
    {
        var user1 = new UserEntity(ValidUsername, ValidSub);
        var user2 = new UserEntity(ValidUsername, ValidSub);

        Assert.That(user1.GetHashCode(), Is.Not.EqualTo(user2.GetHashCode()));
    }

    [Test]
    public void ChangeUsername_ValidUsername_UpdatesUsername()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        var newUsername = new Username("bob");

        user.ChangeUsername(newUsername);

        Assert.That(user.Username, Is.EqualTo(newUsername));
    }

    [Test]
    public void ChangeUsername_DoesNotAffectOtherProperties()
    {
        var user = new UserEntity(ValidUsername, ValidSub);
        var originalId = user.Id;
        string originalSub = user.Sub;

        user.ChangeUsername(new Username("bob"));

        Assert.Multiple(() =>
        {
            Assert.That(user.Id, Is.EqualTo(originalId));
            Assert.That(user.Sub, Is.EqualTo(originalSub));
        });
    }

    [Test]
    public void ChangeUsername_MultipleTimes_UsesLatestValue()
    {
        var user = new UserEntity(ValidUsername, ValidSub);

        user.ChangeUsername(new Username("bob"));
        user.ChangeUsername(new Username("charlie"));

        Assert.That(user.Username.Value, Is.EqualTo("charlie"));
    }
}
