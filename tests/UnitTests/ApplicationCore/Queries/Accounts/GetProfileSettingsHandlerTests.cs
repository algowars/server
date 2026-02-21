using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Accounts.GetProfileSettings;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Accounts;

[TestFixture]
public sealed class GetProfileSettingsHandlerTests
{
    private Mock<IAccountRepository> _repository = null!;
    private GetProfileSettingsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IAccountRepository>();
        _handler = new GetProfileSettingsHandler(_repository.Object);
    }

    [Test]
    public async Task Handle_returns_not_found_when_account_does_not_exist()
    {
        _repository
            .Setup(r => r.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountModel?)null);

        var query = new GetProfileSettingsQuery("sub1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Handle_returns_profile_settings_when_account_exists()
    {
        var account = new AccountModel { Username = "user1" };

        _repository
            .Setup(r => r.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var query = new GetProfileSettingsQuery("sub1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Username, Is.EqualTo("user1"));
        });
    }

    [Test]
    public async Task Handle_returns_error_when_exception_is_thrown()
    {
        _repository
            .Setup(r => r.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db failure"));

        var query = new GetProfileSettingsQuery("sub1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Errors, Has.Some.EqualTo("db failure"));
        });
    }
}