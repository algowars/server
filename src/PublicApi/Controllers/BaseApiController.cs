using System.Security.Claims;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected string? GetSub() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    protected string GetSubOrThrow()
    {
        string? sub = GetSub();
        return string.IsNullOrWhiteSpace(sub)
            ? throw new InvalidOperationException(
                "Authenticated user has no sub (NameIdentifier) claim."
            )
            : sub;
    }

    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(result.Errors),
            ResultStatus.Unauthorized => Unauthorized(result.Errors),
            ResultStatus.Forbidden => Forbid(),
            ResultStatus.Invalid => BadRequest(result.Errors),
            _ => StatusCode(500, "An unexpected error occurred."),
        };
    }
}
