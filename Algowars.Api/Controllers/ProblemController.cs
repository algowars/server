using Algowars.Api.Attributes;
using Algowars.Api.RateLimiting;
using Algowars.Api.Requests.Problem;
using Algowars.Application;
using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;
using Algowars.Application.Services.Problems;
using Algowars.Application.Services.Submissions;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting(WellKnownPolicies.General)]
public sealed class ProblemController(IProblemService problemService, ISubmissionService submissionService, UserContext userContext) : ControllerBase
{
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemWithSetupsDto>> GetProblemBySlug(
        string slug, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await problemService.GetProblemWithSetupsBySlug(slug, cancellationToken));
    }

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

    [HttpGet("{slug}/setup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemSetupDto>> GetProblemSetup(
        string slug, [FromQuery] Guid languageVersionId, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await problemService.GetProblemSetupAsync(slug, languageVersionId, cancellationToken));
    }

    [HttpGet("{slug}/submissions")]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResult<ProblemSubmissionDto>>> GetSubmissions(
        string slug, [FromQuery] GetProblemSubmissionsRequest query, CancellationToken cancellationToken)
    {
        if (userContext.User is null)
            return this.ToActionResult(Result.Unauthorized());

        bool includeAllSubmissions = query.Filter == SubmissionFilterType.All;

        return this.ToActionResult(await submissionService.GetSubmissionsByProblemSlugAsync(
            slug,
            new PaginationRequest
            {
                Page = query.Page,
                Size = query.Size,
                Timestamp = query.Timestamp
            },
            userContext.User.Id,
            includeAllSubmissions,
            cancellationToken));
    }
}