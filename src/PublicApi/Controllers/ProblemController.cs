using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PublicApi.Attributes;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class ProblemController(
    IProblemAppService problemAppService,
    ISubmissionAppService submissionAppService,
    IAccountContext accountContext
) : BaseApiController
{
    [HttpGet("slug/{slug}")]
    [EnableRateLimiting("Short")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlugAsync(
        string slug,
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
    [ProducesResponseType(typeof(IEnumerable<ProgrammingLanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAvailableLanguagesAsync(CancellationToken cancellationToken)
    {
        return ToActionResult(
            await problemAppService.GetAvailableLanguagesAsync(cancellationToken)
        );
    }

    [HttpGet]
    [EnableRateLimiting("Medium")]
    [ProducesResponseType(typeof(PaginatedResult<ProblemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ProblemSetupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpGet("{problemId:guid}/submissions")]
    [EnableRateLimiting("Short")]
    [ProducesResponseType(typeof(PaginatedResult<SubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubmissionsAsync(
        Guid problemId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 25,
        [FromQuery] DateTime? timestamp = null,
        [FromQuery] bool mySolution = false,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || size < 1)
        {
            return BadRequest("Page and size must be greater than 0.");
        }

        var request = new GetSubmissionsPaginatedRequest
        {
            ProblemId = problemId,
            Page = page,
            Size = size,
            Timestamp = timestamp ?? DateTime.UtcNow,
            FilterByUserId = mySolution ? accountContext.Account.Id : null,
            AcceptedOnly = !mySolution,
        };

        return ToActionResult(
            await submissionAppService.GetSubmissionsPaginatedAsync(request, cancellationToken)
        );
    }
}