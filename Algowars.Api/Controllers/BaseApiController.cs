using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public abstract class BaseApiController : ControllerBase
{
    protected string? GetSub()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

    protected IActionResult ToActionResult<T>(Result<T> result) => result.Status switch
    {
        ResultStatus.Ok => Ok(result.Value),
        ResultStatus.NotFound => NotFound(),
        ResultStatus.Unauthorized => Unauthorized(),
        ResultStatus.Forbidden => Forbid(),
        ResultStatus.Invalid => BadRequest(result.ValidationErrors),
        ResultStatus.Error => StatusCode(500, new { result.Errors }),
        _ => StatusCode(500, new { Message = "An unexpected error occurred." }),
    };

    protected IActionResult ToActionResult(Result result) => result.Status switch
    {
        ResultStatus.Ok => Ok(),
        ResultStatus.NotFound => NotFound(),
        ResultStatus.Unauthorized => Unauthorized(),
        ResultStatus.Forbidden => Forbid(),
        ResultStatus.Invalid => BadRequest(result.ValidationErrors),
        ResultStatus.Error => StatusCode(500, new { result.Errors }),
        _ => StatusCode(500, new { Message = "An unexpected error occurred." }),
    };
}
