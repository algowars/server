using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed partial class CreateSubmissionHandler(
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
            var isSuccessful = await submissionRepository.SaveAsync(submission, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.ToString());
        }
    }
}
