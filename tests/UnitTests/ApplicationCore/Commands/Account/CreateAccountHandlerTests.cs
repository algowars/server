using ApplicationCore.Commands.Accounts.CreateAccount;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Account;

[TestFixture]
public sealed class CreateAccountHandlerTests
{
    private Mock<IAccountRepository> _accounts = null!;
    private Mock<ILogger<CreateAccountHandler>> _logger = null!;
    private CreateAccountHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _accounts = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<CreateAccountHandler>>();
        _handler = new CreateAccountHandler(_accounts.Object, _logger.Object);
    }

    [Test]
    public async Task Handle_creates_account_successfully()
    {
        _accounts
            .Setup(a => a.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _accounts
            .Setup(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Task.CompletedTask);

        var command = new CreateAccountCommand("user1", "sub1", "http://image.url");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
        _accounts.Verify(
            a =>
                a.AddAsync(
                    It.Is<AccountModel>(acc => acc.Username == "user1" && acc.Sub == "sub1"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _accounts.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_returns_error_when_exception_occurs()
    {
        _accounts
            .Setup(a => a.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var command = new CreateAccountCommand("user2", "sub2", "");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.EqualTo("Unexpected error creating account."));
    }

    [Test]
    public void Constructor_throws_if_accounts_null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var createAccountHandler = new CreateAccountHandler(null!, _logger.Object);
        });
    }

    [Test]
    public void Constructor_throws_if_logger_null()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var createAccountHandler = new CreateAccountHandler(_accounts.Object, null!);
        });
    }
}
