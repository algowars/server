using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Accounts.GetProfileAggregate;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Accounts;

[TestFixture]
public sealed class GetProfileAggregateHandlerTests
{
    private Mock<IAccountRepository> _repository = null!;
    private GetProfileAggregateHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IAccountRepository>();
        _handler = new GetProfileAggregateHandler(_repository.Object);
    }

    [Test]
    public async Task Handle_returns_invalid_when_username_is_empty()
    {
        var query = new GetProfileAggregateQuery("");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(result.ValidationErrors, Has.Exactly(1).Items);
            Assert.That(result.ValidationErrors.First().Identifier, Is.EqualTo("Username"));
        });
    }

    [Test]
    public async Task Handle_returns_not_found_when_account_does_not_exist()
    {
        _repository
            .Setup(r => r.GetByUsernameAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountModel?)null);

        var query = new GetProfileAggregateQuery("user1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Handle_returns_profile_aggregate_when_account_exists()
    {
        var account = new AccountModel
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            ImageUrl = "http://image.url",
            CreatedOn = DateTime.UtcNow,
        };

        _repository
            .Setup(r => r.GetByUsernameAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var query = new GetProfileAggregateQuery("user1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Profile.Username, Is.EqualTo("user1"));
            Assert.That(result.Value.Profile.ImageUrl, Is.EqualTo("http://image.url"));
            Assert.That(result.Value.Profile.Id, Is.EqualTo(account.Id));
        });
    }

    [Test]
    public async Task Handle_returns_error_when_exception_is_thrown()
    {
        _repository
            .Setup(r => r.GetByUsernameAsync("user1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db failure"));

        var query = new GetProfileAggregateQuery("user1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Errors, Has.Some.EqualTo("db failure"));
        });
    }
}
