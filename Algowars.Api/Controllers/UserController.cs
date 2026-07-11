using Algowars.Api.Attributes;
using Algowars.Api.Requests.User;
using Algowars.Api.Responses.User;
using Algowars.Application;
using Algowars.Application.Services.Users;
using Algowars.Application.Users.Dtos;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class UserController(IUserService userService, UserContext userContext) : ControllerBase
{
    [HttpGet]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> GetAccount()
    {
        if (userContext.User is null)
            return NotFound();

        return Ok(UserResponse.FromDto(userContext.User));
    }

    [HttpPut]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Unit>> UpsertAccount(
        [FromBody] UpsertUserRequest request, CancellationToken cancellationToken)
    {
        string? sub = GetSub();

        if (string.IsNullOrEmpty(sub))
        {
            return this.ToActionResult(Result<Unit>.Invalid(new ValidationError("sub", "User sub is missing")));
        }

        return this.ToActionResult(await userService.UpsertAccountAsync(
        sub,
        new UpsertUserDto(request.Username, request.Picture, request.Bio),
        cancellationToken));
    }

    private string? GetSub() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

}