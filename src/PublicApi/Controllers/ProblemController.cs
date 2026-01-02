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

    [HttpGet]
    public async Task<IActionResult> GetPageableAsync(
        [FromQuery] DateTime timestamp,
        [FromQuery] int page = 1,
        [FromQuery] int size = 25,
        CancellationToken cancellationToken = default
    )
    {
        if (page < 1 || size < 1)
        {
            return BadRequest("Page and size must be greater than 0.");
        }

        return ToActionResult(
            await problemAppService.GetProblemsPaginatedAsync(
                page,
                size,
                timestamp,
                cancellationToken
            )
        );
    }

    [HttpGet("{problemId:guid}/setup")]
    public async Task<IActionResult> GetProblemSetupAsync(
        Guid problemId,
        [FromQuery] int languageVersionId,
        CancellationToken cancellationToken
    )
    {
        return ToActionResult(
            await problemAppService.GetProblemSetupAsync(
                problemId,
                languageVersionId,
                cancellationToken
            )
        );
    }
}
