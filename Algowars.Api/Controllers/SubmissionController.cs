using System.Linq;
using Algowars.Api.Attributes;
using Algowars.Api.Authorization;
using Algowars.Api.RateLimiting;
using Algowars.Api.Requests.Submission;
using Algowars.Application;
using Algowars.Application.Services.Submissions;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting(WellKnownPolicies.Submissions)]
public sealed class SubmissionController(ISubmissionService submissionService, UserContext userContext) : ControllerBase
{
    [HttpPost("run")]
    [RequireUser]
    [RequirePermission(WellKnownPermissions.CreateSubmission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> CreateRunSubmission(
        [FromBody] CreateRunSubmissionRequest request, CancellationToken cancellationToken)
    {
        if (userContext.User is null)
            return this.ToActionResult(Result<Guid>.Unauthorized());

        return this.ToActionResult(await submissionService.CreateSubmissionAsync(
            new CreateSubmissionDto(
                request.ProblemSetupId,
                Domain.Submissions.Enums.SubmissionType.Run,
                request.Code,
                userContext.User.Id,
                request.CustomTestCases?.Select(tc => new CreateSubmissionCustomTestCaseDto(tc.Inputs)).ToArray()),
            cancellationToken));
    }

    [HttpPost("grade")]
    [RequireUser]
    [RequirePermission(WellKnownPermissions.CreateSubmission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> CreateGradeSubmission(
        [FromBody] CreateGradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        if (userContext.User is null)
            return this.ToActionResult(Result<Guid>.Unauthorized());

        return this.ToActionResult(await submissionService.CreateSubmissionAsync(
            new CreateSubmissionDto(
                request.ProblemSetupId,
                Domain.Submissions.Enums.SubmissionType.Submit,
                request.Code,
                userContext.User.Id,
                null),
            cancellationToken));
    }

    [HttpGet("{submissionId:guid}")]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmissionStatusDto>> GetSubmissionStatus(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        if (userContext.User is null)
            return this.ToActionResult(Result<SubmissionStatusDto>.Unauthorized());

        return this.ToActionResult(await submissionService.GetSubmissionStatusAsync(
            submissionId,
            userContext.User.Id,
            cancellationToken));
    }
}
