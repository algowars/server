using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed partial class CreateSubmissionHandler(
    IProblemRepository problemRepository,
    ISubmissionRepository submissionRepository,
    ICodeBuilderService codeBuilderService,
    ICodeExecutionService codeExecutionService,
    IValidator<CreateSubmissionCommand> validator
) : AbstractCommandHandler<CreateSubmissionCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(
        CreateSubmissionCommand request,
        CancellationToken cancellationToken
    )
    {
        var setup = await problemRepository.GetProblemSetupAsync(
            request.ProblemSetupId,
            cancellationToken
        );

        if (setup is null)
        {
            return Result.NotFound("Problem Setup not found.");
        }

        var contexts = setup
            .TestSuites.SelectMany(s => s.TestCases)
            .Select(tc => new CodeBuilderContext
            {
                InitialCode = setup.InitialCode,
                Template = setup.HarnessTemplate.Template,
                FunctionName = setup.FunctionName ?? string.Empty,
                LanguageVersionId = setup.LanguageVersion?.Id,
                Inputs = tc.Input,
                ExpectedOutput = tc.ExpectedOutput,
                InputTypeName = null,
            });

        var buildResult = await codeBuilderService.BuildAsync(contexts);

        if (!buildResult.IsSuccess)
        {
            return Result.Invalid(buildResult.ValidationErrors);
        }

        var submissionResult = await codeExecutionService.ExecuteAsync(
            new CodeExecutionContext
            {
                Setup = setup,
                Code = request.Code,
                BuiltResults = buildResult.Value,
                CreatedById = request.CreatedById,
            },
            cancellationToken
        );

        if (!submissionResult.IsSuccess)
        {
            return Result.Error(submissionResult.Errors.ToString());
        }

        try
        {
            await submissionRepository.SaveAsync(submissionResult.Value, CancellationToken.None);

            return Result.Success(submissionResult.Value.Id);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.ToString());
        }
    }
}
