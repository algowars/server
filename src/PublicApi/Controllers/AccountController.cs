using ApplicationCore.Commands.Accounts.UpdateUsername;
using ApplicationCore.Commands.Accounts.UpsertAccount;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PublicApi.Attributes;
using PublicApi.Contracts.Account;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed partial class AccountController(
    IAccountAppService accountAppService,
    IAccountContext accountContext
) : BaseApiController
{
    [HttpPut]
    [Authorize]
    [EnableRateLimiting("ExtraShort")]
    [ProducesResponseType(typeof(AccountUpsertResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpsertAccountAsync(
        [FromBody] UpsertAccountDto request,
        CancellationToken cancellationToken
    )
    {
        string? sub = GetSub();

        if (string.IsNullOrEmpty(sub))
        {
            return Unauthorized();
        }

        var result = await accountAppService.UpsertAccountAsync(sub, request.ImageUrl, cancellationToken);

        return ToActionResult(result);
    }

    [HttpGet("find/profile/{username}")]
    [EnableRateLimiting("Short")]
    [ProducesResponseType(typeof(ProfileAggregateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileAsync(
        string username,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required");
        }

        var accountResult = await accountAppService.GetProfileAggregateAsync(
            username,
            cancellationToken
        );

        if (accountResult.IsSuccess)
        {
            return Ok(accountResult.Value);
        }

        string errors = string.Join(", ", accountResult.Errors);

        return BadRequest(errors);
    }

    [HttpGet("find/profile")]
    [Authorize]
    [EnableRateLimiting("Medium")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken)
    {
        string? sub = GetSub();

        if (string.IsNullOrEmpty(sub))
        {
            return Unauthorized();
        }

        var result = await accountAppService.GetAccountBySubAsync(sub, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        IEnumerable<string> permissions =
        [
            .. User.Claims.Where(c => c.Type == "permissions").Select(c => c.Value),
        ];

        return Ok(result.Value with { Permissions = permissions });
    }

    [HttpPut("username")]
    [Authorize]
    [RequiresAccount]
    [EnableRateLimiting("ExtraShort")]
    [ProducesResponseType(typeof(UpdateUsernameResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUsernameAsync(
        [FromBody] UpdateUsernameDto request,
        CancellationToken cancellationToken
    )
    {
        if (accountContext.Account is null)
        {
            return Unauthorized();
        }

        var result = await accountAppService.UpdateUsernameAsync(
            accountContext.Account.Id,
            request.Username,
            accountContext.Account.UsernameLastChangedAt,
            cancellationToken
        );

        return ToActionResult(result);
    }
}