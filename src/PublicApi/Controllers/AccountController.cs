using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PublicApi.Contracts.Account;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed partial class AccountController(IAccountAppService accountAppService)
    : BaseApiController
{
    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ExtraShort")]
    public async Task<IActionResult> CreateAccountAsync(
        [FromBody] CreateAccountDto createAccountDto,
        CancellationToken cancellationToken
    )
    {
        string? sub = GetSub();

        if (sub is null)
        {
            return Unauthorized();
        }

        var accountResult = await accountAppService.CreateAsync(
            createAccountDto.Username,
            sub,
            createAccountDto.ImageUrl ?? "",
            cancellationToken
        );

        return ToActionResult(accountResult);
    }

    [HttpGet("find/profile/{username}")]
    [EnableRateLimiting("Short")]
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
    public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken)
    {
        string? sub = GetSub();

        if (sub is null)
        {
            return Unauthorized();
        }

        var permissions = User.Claims.Where(c => c.Type == "permissions").Select(c => c.Value);

        var accountResult = await accountAppService.GetAccountBySubAsync(sub, cancellationToken);

        if (accountResult.IsSuccess)
        {
            return Ok(accountResult.Value);
        }

        string errors = string.Join(", ", accountResult.Errors);

        return BadRequest(errors);
    }
}
