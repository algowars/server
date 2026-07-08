
using Algowars.Api.Requests.Problem;
using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;
using Algowars.Application.Services.Problems;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;
namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ProblemController(IProblemService problemService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResult<ProblemDto>>> GetProblems(
        [FromQuery] GetProblemsPageableRequest query, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await problemService.GetProblemsPageableAsync(new PaginationRequest
        {
            Page = query.Page,
            Size = query.Size,
            Timestamp = query.Timestamp
        }, cancellationToken));
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemWithSetupsDto>> GetProblemBySlug(string slug, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await problemService.GetProblemWithSetupsBySlug(slug, cancellationToken));
    }

    [HttpGet("{slug}/setup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemSetupDto>> GetProblemSetup(string slug, [FromQuery] Guid languageVersionId, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await problemService.GetProblemSetupAsync(slug, languageVersionId, cancellationToken));
    }
}
