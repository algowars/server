using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Logging;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Commands.Accounts.CreateAccount;

public sealed partial class CreateAccountHandler(
    IAccountRepository accounts,
    ILogger<CreateAccountHandler> logger
) : ICommandHandler<CreateAccountCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken
    )
    {
        var id = Guid.NewGuid();

        var account = new AccountModel
        {
            Id = id,
            Username = request.Username,
            Sub = request.Sub,
            ImageUrl = request.ImageUrl,
            LastModifiedById = null,
        };

        try
        {
            await accounts.AddAsync(account, cancellationToken);
        }
        catch (Exception ex)
        {
            LogCreateFailed(logger, request.Username, request.Sub, ex.Message);
            return Result<Guid>.Error("Unexpected error creating account.");
        }

        LogCreated(logger, id, request.Sub);
        return Result.Success(id);
    }

    [LoggerMessage(
        EventId = LoggingEventIds.Accounts.Created,
        Level = LogLevel.Information,
        Message = "Created account {accountId} for sub {sub}"
    )]
    private static partial void LogCreated(ILogger logger, Guid accountId, string sub);

    [LoggerMessage(
        EventId = LoggingEventIds.Accounts.CreateFailed,
        Level = LogLevel.Error,
        Message = "Failed to create account for {username}/{sub}. DB message: {dbMessage}"
    )]
    private static partial void LogCreateFailed(
        ILogger logger,
        string username,
        string sub,
        string dbMessage
    );
}
