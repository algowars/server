using ApplicationCore.Commands.Accounts.CreateAccount;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Account;

[TestFixture]
public sealed class CreateAccountHandlerTests
{
    private Mock<IAccountRepository> _accounts;
    private Mock<ILogger<CreateAccountHandler>> _logger;
    private Mock<IValidator<CreateAccountCommand>> _validator;

    private CreateAccountHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _accounts = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<CreateAccountHandler>>();
        _validator = new Mock<IValidator<CreateAccountCommand>>();

        _validator
            .Setup(v =>
                v.ValidateAsync(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _handler = new CreateAccountHandler(_accounts.Object, _logger.Object, _validator.Object);
    }

    [Test]
    public async Task Handle_creates_account_successfully()
    {
        _accounts
            .Setup(a => a.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateAccountCommand("user1", "sub1", "http://image.url");

        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
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
        }
    }

    [Test]
    public async Task Handle_returns_error_when_exception_occurs()
    {
        _accounts
            .Setup(a => a.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var command = new CreateAccountCommand("user2", "sub2", "");

        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Some.EqualTo("Unexpected error creating account."));
        }
    }
}
