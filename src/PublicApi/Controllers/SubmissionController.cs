using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
