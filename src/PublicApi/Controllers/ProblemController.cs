using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class ProblemController(IProblemAppService problemAppService) : BaseApiController
{
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlugAsync(
        string slug,
        [FromQuery] int preferredLanguageId,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Slug is required.");
        }

        var problemResult = await problemAppService.GetProblemBySlugAsync(slug, cancellationToken);

        if (problemResult.IsSuccess)
        {
            return Ok(problemResult.Value);
        }

        string errors = string.Join(", ", problemResult.Errors);

        return BadRequest(errors);
    }
}
