using ApplicationCore.Commands.Accounts.CreateAccount;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Account;

[TestFixture]
public sealed class CreateAccountValidatorTests
{
    private Mock<IAccountRepository> _accounts = null!;
    private CreateAccountValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _accounts = new Mock<IAccountRepository>();
        _validator = new CreateAccountValidator(_accounts.Object);
    }

    [Test]
    public async Task Validator_passes_for_valid_command()
    {
        _accounts
            .Setup(a => a.GetByUsernameAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountModel?)null);
        _accounts
            .Setup(a => a.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountModel?)null);
        _accounts
            .Setup(a => a.GetByUsernameOrSubAsync("user1", "sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountModel?)null);

        var command = new CreateAccountCommand("user1", "sub1", "http://example.com");
        var result = await _validator.ValidateAsync(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validator_fails_if_username_is_invalid()
    {
        var command = new CreateAccountCommand("user$invalid", "sub1", "");
        var result = await _validator.ValidateAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Some.Property("ErrorMessage").Contains("Username contains invalid characters")
            );
        });
    }

    [Test]
    public async Task Validator_fails_if_username_already_exists()
    {
        _accounts
            .Setup(a => a.GetByUsernameAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountModel() { Username = "test" });

        var command = new CreateAccountCommand("user1", "sub1", "");
        var result = await _validator.ValidateAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Some.Property("ErrorMessage").Contains("Username already exists")
            );
        });
    }

    [Test]
    public async Task Validator_fails_if_sub_already_exists()
    {
        _accounts
            .Setup(a => a.GetBySubAsync("sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountModel() { Username = "test" });

        var command = new CreateAccountCommand("user1", "sub1", "");
        var result = await _validator.ValidateAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Some.Property("ErrorMessage").Contains("Account already exists")
            );
        });
    }

    [Test]
    public async Task Validator_fails_if_username_or_sub_exists()
    {
        _accounts
            .Setup(a => a.GetByUsernameOrSubAsync("user1", "sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountModel() { Username = "test" });

        var command = new CreateAccountCommand("user1", "sub1", "");
        var result = await _validator.ValidateAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Some.Property("ErrorMessage").Contains("Username already exists")
            );
        });
    }

    [Test]
    public async Task Validator_fails_if_imageUrl_is_invalid()
    {
        var command = new CreateAccountCommand("user1", "sub1", "ftp://invalid.url");
        var result = await _validator.ValidateAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Some.Property("ErrorMessage").Contains("ImageUrl must be a valid URL")
            );
        });
    }
}
