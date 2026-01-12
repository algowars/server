using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.Contracts.Account;
using PublicApi.Contracts.Submission;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SubmissionController(
    IAccountAppService accountAppService,
    ISubmissionAppService submissionAppService
) : BaseApiController
{
    [HttpPost("execute")]
    [Authorize]
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
            return Unauthorized();
        }

        var submissionResult = await submissionAppService.ExecuteAsync(
            createSubmissionDto.ProblemSetupId,
            createSubmissionDto.Code,
            accountResult.Value.Id,
            cancellationToken
        );

        return ToActionResult(submissionResult);
    }

    [HttpGet("{submissionId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSubmissionsAsync(
        [FromRoute] Guid submissionId,
        CancellationToken cancellationToken
    )
    {
        var submissionResult = await submissionAppService.GetByIdAsync(
            submissionId,
            cancellationToken
        );
        return ToActionResult(submissionResult);
    }
}
