
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
}
