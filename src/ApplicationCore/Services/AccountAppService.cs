using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Commands.Accounts.CreateAccount;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Accounts.GetAccountBySub;
using ApplicationCore.Queries.Accounts.GetProfileAggregate;
using ApplicationCore.Queries.Accounts.GetProfileSettings;
using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Services;

public sealed class AccountAppService(IMediator mediator) : IAccountAppService
{
    public async Task<Result<Guid>> CreateAsync(
        string username,
        string sub,
        string imageUrl,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateAccountCommand(username, sub, imageUrl);

        var result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public async Task<Result<AccountDto>> GetAccountBySubAsync(
        string sub,
        CancellationToken cancellationToken
    )
    {
        var query = new GetAccountBySubQuery(sub);

        return await mediator.Send(query, cancellationToken);
    }

    public async Task<Result<ProfileAggregateDto>> GetProfileAggregateAsync(
        string username,
        CancellationToken cancellationToken
    )
    {
        var query = new GetProfileAggregateQuery(username);

        return await mediator.Send(query, cancellationToken);
    }

    public async Task<ProfileSettingsDto?> GetProfileSettingsAsync(
        string sub,
        CancellationToken cancellationToken
    )
    {
        var query = new GetProfileSettingsQuery(sub);

        return await mediator.Send(query, cancellationToken);
    }
}
