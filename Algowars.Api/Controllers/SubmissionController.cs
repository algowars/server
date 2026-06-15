using Algowars.Api.Attributes;
using Algowars.Api.Context;
using Algowars.Application.Commands.Submissions.CreateSubmission;
using Algowars.Application.Dtos.Submissions;
using Algowars.Application.Queries.Submissions.GetSubmissionStatus;
using Algowars.Domain.Submissions.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/submissions")]
[ApiVersion("1.0")]
public sealed class SubmissionController(ISender sender, IUserContext userContext) : BaseApiController
{
    [HttpPost("execute")]
    [Authorize]
    [RequiresUser]
    [EnableRateLimiting("User_5:60")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubmissionAsync(
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (userContext.User is null)
            return Unauthorized();

        var result = await sender.Send(new CreateSubmissionCommand(
            userContext.User.Id,
            request.ProblemVersionId,
            request.LanguageVersionId,
            request.Type,
            request.SourceCode,
            request.TestCaseIds), cancellationToken);

        return ToActionResult(result);
    }

    [HttpGet("{submissionId:guid}")]
    [Authorize]
    [RequiresUser]
    [EnableRateLimiting("User_30:60")]
    [ProducesResponseType(typeof(SubmissionStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubmissionStatusAsync(Guid submissionId, CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetSubmissionStatusQuery(submissionId), cancellationToken));
}

public sealed record CreateSubmissionRequest(
    Guid ProblemVersionId,
    Guid LanguageVersionId,
    SubmissionType Type,
    string SourceCode,
    IEnumerable<Guid> TestCaseIds);
