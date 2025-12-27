using System.Security.Claims;
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
}
