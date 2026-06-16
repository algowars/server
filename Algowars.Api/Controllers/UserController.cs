using Algowars.Api.Attributes;
using Algowars.Api.Context;
using Algowars.Application.Commands.Users.UpsertUser;
using Algowars.Application.Dtos.Users;
using Algowars.Application.Queries.Users.GetProfileAggregate;
using Algowars.Application.Queries.Users.GetProfileSettings;
using Algowars.Application.Queries.Users.GetUserBySub;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
public sealed class UserController(ISender sender) : BaseApiController
{
    [HttpPut]
    [Authorize]
    [EnableRateLimiting("User_10:60")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpsertUserAsync(
        [FromBody] UpsertUserRequest? request,
        CancellationToken cancellationToken)
    {
        string? sub = GetSub();
        if (string.IsNullOrEmpty(sub))
            return Unauthorized();

        string? imageUrl = request?.ImageUrl ?? User.FindFirst("picture")?.Value;
        var result = await sender.Send(new UpsertUserCommand(sub, imageUrl, request?.Username), cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("find/profile/{username}")]
    [EnableRateLimiting("User_30:60")]
    [ProducesResponseType(typeof(ProfileAggregateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
            return BadRequest("Username is required");

        return ToActionResult(await sender.Send(new GetProfileAggregateQuery(username), cancellationToken));
    }

    [HttpGet("me")]
    [Authorize]
    [RequiresUser]
    [EnableRateLimiting("User_30:60")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeAsync(CancellationToken cancellationToken)
    {
        string? sub = GetSub();
        if (string.IsNullOrEmpty(sub))
            return Unauthorized();

        var result = await sender.Send(new GetUserBySubQuery(sub), cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("settings")]
    [Authorize]
    [RequiresUser]
    [EnableRateLimiting("User_30:60")]
    [ProducesResponseType(typeof(ProfileSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken)
    {
        string? sub = GetSub();
        if (string.IsNullOrEmpty(sub))
            return Unauthorized();

        return ToActionResult(await sender.Send(new GetProfileSettingsQuery(sub), cancellationToken));
    }
}

public sealed record UpsertUserRequest(string? Username, string? ImageUrl);
