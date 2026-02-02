using ApplicationCore.Domain.Accounts;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Accounts.GetAccountBySub;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Accounts;

[TestFixture]
public sealed class GetAccountBySubHandlerTests
{
    private Mock<IAccountRepository> _repository = null!;
    private GetAccountBySubHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IAccountRepository>();
        _handler = new GetAccountBySubHandler(_repository.Object);
    }

    [Test]
    public async Task Handle_returns_invalid_when_sub_is_empty()
    {
        var query = new GetAccountBySubQuery(string.Empty);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(result.ValidationErrors.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Handle_returns_not_found_when_account_does_not_exist()
    {
        _repository
            .Setup(r => r.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountModel?)null);

        var query = new GetAccountBySubQuery("sub1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Handle_returns_account_dto_when_account_exists()
    {
        var account = new AccountModel()
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            Sub = "sub1",
            ImageUrl = "http://image.url",
            CreatedOn = DateTime.UtcNow,
        };

        _repository
            .Setup(r => r.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var query = new GetAccountBySubQuery("sub1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Id, Is.EqualTo(account.Id));
            Assert.That(result.Value.Username, Is.EqualTo(account.Username));
            Assert.That(result.Value.ImageUrl, Is.EqualTo(account.ImageUrl));
            Assert.That(result.Value.CreatedOn, Is.EqualTo(account.CreatedOn));
        });
    }

    [Test]
    public async Task Handle_returns_error_when_exception_is_thrown()
    {
        _repository
            .Setup(r => r.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var query = new GetAccountBySubQuery("sub1");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Errors, Has.Some.EqualTo("db error"));
        });
    }
}