using Algowars.Application.Services.Users;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> UpsertAccount(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    => this.ToActionResult(await userService.CreateUserAsync(
        request.Username,
        request.Sub,
        request.ImageUrl,
        cancellationToken));
    
}

public sealed record CreateUserRequest(string Username, string Sub, string? ImageUrl);
