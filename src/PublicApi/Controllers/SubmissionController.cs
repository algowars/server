using ApplicationCore.Common.Pagination;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PublicApi.Contracts.Submission;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class SubmissionController(
    IAccountAppService accountAppService,
    ISubmissionAppService submissionAppService
) : BaseApiController
{
    [HttpGet("{submissionId:Guid}")]
    [Authorize]
    public async Task<IActionResult> GetSubmissionAsync(
        Guid submissionId,
        CancellationToken cancellationToken
    )
    {
        return ToActionResult(
            await submissionAppService.GetSubmissionAsync(submissionId, cancellationToken)
        );
    }

    [HttpGet("problem/{problemId:Guid}")]
    [Authorize]
    public async Task<IActionResult> GetProblemSubmissionsAsync(
        Guid problemId,
        [FromQuery] PaginationRequest paginationRequest,
        CancellationToken cancellationToken
    )
    {
        return ToActionResult(
            await submissionAppService.GetSubmissionsAsync(
                problemId,
                paginationRequest,
                cancellationToken
            )
        );
    }

    [HttpPost("execute")]
    [Authorize]
    [EnableRateLimiting("SubmissionDaily")]
    public async Task<IActionResult> CreateSubmissionAsync(
        [FromBody] CreateSubmissionDto createSubmissionDto,
        CancellationToken cancellationToken
    )
    {
        string? sub = GetSub();

        if (sub is null)
        {
            return Unauthorized();
        }

        var accountResult = await accountAppService.GetAccountBySubAsync(sub, cancellationToken);

        if (!accountResult.IsSuccess)
        {
            return ToActionResult(accountResult);
        }

        return ToActionResult(
            await submissionAppService.CreateAsync(
                createSubmissionDto.ProblemSetupId,
                createSubmissionDto.Code,
                accountResult.Value.Id,
                cancellationToken
            )
        );
    }
}
