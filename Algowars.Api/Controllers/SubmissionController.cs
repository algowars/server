using Algowars.Api.Attributes;
using Algowars.Api.Requests.Submission;
using Algowars.Application;
using Algowars.Application.Services.Submissions;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SubmissionController(ISubmissionService submissionService, UserContext userContext) : ControllerBase
{
    [HttpPost]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Unit>> CreateSubmission(
        [FromBody] CreateSubmissionRequest request, CancellationToken cancellationToken)
    {
        if (userContext.User is null)
            return this.ToActionResult(Result<Unit>.Unauthorized());

        return this.ToActionResult(await submissionService.CreateSubmissionAsync(
            new CreateSubmissionDto(
                request.ProblemSetupId,
                request.Type,
                request.Code,
                userContext.User.Id),
            cancellationToken));
    }
}
