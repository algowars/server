using Algowars.Api.Attributes;
using Algowars.Api.Requests.User;
using Algowars.Application;
using Algowars.Application.Services.Users;
using Algowars.Application.Users.Dtos;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController(IUserService userService, UserContext userContext) : ControllerBase
{
    [HttpPut]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Unit>> UpsertAccount(
        [FromBody] UpsertUserRequest request, CancellationToken cancellationToken)
    => this.ToActionResult(await userService.UpsertAccountAsync(
        userContext.User!.Id,
        new UpsertUserDto(request.ImageUrl, request.Bio),
        cancellationToken));
}
