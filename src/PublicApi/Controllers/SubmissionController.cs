using ApplicationCore.Domain.Accounts;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PublicApi.Attributes;
using PublicApi.Contracts.Submission;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class SubmissionController(
    IAccountContext accountContext,
    ISubmissionAppService submissionAppService
) : BaseApiController
{
    [HttpPost("execute")]
    [Authorize]
    [RequiresAccount]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubmissionAsync(
        [FromBody] CreateSubmissionDto createSubmissionDto,
        CancellationToken cancellationToken
    )
    {
        if (accountContext.Account is null)
        {
            return Unauthorized();
        }

        return ToActionResult(
            await submissionAppService.CreateAsync(
                createSubmissionDto.ProblemSetupId,
                createSubmissionDto.Code,
                accountContext.Account.Id,
                cancellationToken
            )
        );
    }
}