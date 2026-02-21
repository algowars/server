using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed class CreateSubmissionHandler(
    ISubmissionRepository submissionRepository,
    IValidator<CreateSubmissionCommand> validator
) : AbstractCommandHandler<CreateSubmissionCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(
        CreateSubmissionCommand request,
        CancellationToken cancellationToken
    )
    {
        var submission = new SubmissionModel
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            ProblemSetupId = request.ProblemSetupId,
            CreatedOn = DateTime.UtcNow,
            CreatedById = request.CreatedById,
        };

        try
        {
            await submissionRepository.SaveAsync(submission, cancellationToken);

            return Result.Success(submission.Id);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}