using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class ProblemController(
    IProblemAppService problemAppService,
    IAccountAppService accountAppService
) : BaseApiController
{
    [HttpPost]
    [EnableRateLimiting("Short")]
    public async Task<IActionResult> CreateProblemAsync(
        [FromBody] CreateProblemDto createProblemDto,
        CancellationToken cancellationToken
    )
    {
        var account = await accountAppService.GetAccountBySubAsync(
            GetSub() ?? "",
            cancellationToken
        );

        return ToActionResult(
            await problemAppService.CreateProblemAsync(
                createProblemDto,
                account.Value.Id,
                cancellationToken
            )
        );
    }

    [HttpGet("slug/{slug}")]
    [EnableRateLimiting("Short")]
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

    [HttpGet("languages")]
    [EnableRateLimiting("Short")]
    [Authorize(Policy = "read:languages")]
    public async Task<IActionResult> GetAvailableLanguagesAsync(CancellationToken cancellationToken)
    {
        return ToActionResult(
            await problemAppService.GetAvailableLanguagesAsync(cancellationToken)
        );
    }

    [HttpGet]
    [EnableRateLimiting("Medium")]
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
    [EnableRateLimiting("ExtraShort")]
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