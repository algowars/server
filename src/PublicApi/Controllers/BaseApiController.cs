using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            ResultStatus.NotFound => NotFound(
                new { Message = "Resource not found", result.Errors }
            ),
            ResultStatus.Unauthorized => Unauthorized(
                new { Message = "Unauthorized", result.Errors }
            ),
            ResultStatus.Forbidden => Forbid(),
            ResultStatus.Invalid => BadRequest(
                new
                {
                    Message = result.ValidationErrors is not null
                        ? string.Join(
                            ", ",
                            result.ValidationErrors.Select(e =>
                                string.IsNullOrWhiteSpace(e.Identifier)
                                    ? e.ErrorMessage
                                    : $"{e.Identifier}: {e.ErrorMessage}"
                            )
                        )
                        : "Invalid request.",
                    Errors = result.ValidationErrors?.Select(e => new
                    {
                        Field = e.Identifier,
                        Error = e.ErrorMessage,
                    }),
                }
            ),
            _ => StatusCode(
                500,
                new { Message = "An unexpected error occurred.", Errors = result.Errors }
            ),
        };
    }
}