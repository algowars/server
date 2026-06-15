using Algowars.Application.Common.Pagination;
using Algowars.Application.Dtos.Problems;
using Algowars.Application.Queries.Problems.GetProblemBySlug;
using Algowars.Application.Queries.Problems.GetProblemsPageable;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/problems")]
[ApiVersion("1.0")]
public sealed class ProblemController(ISender sender) : BaseApiController
{
    [HttpGet("slug/{slug}")]
    [EnableRateLimiting("User_30:60")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest("Slug is required.");

        return ToActionResult(await sender.Send(new GetProblemBySlugQuery(slug), cancellationToken));
    }

    [HttpGet]
    [EnableRateLimiting("User_30:60")]
    [ProducesResponseType(typeof(PaginatedResult<ProblemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPageableAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || size < 1)
            return BadRequest("Page and size must be greater than 0.");

        return ToActionResult(await sender.Send(
            new GetProblemsPageableQuery(new PaginationRequest { Page = page, Size = size }),
            cancellationToken));
    }
}
