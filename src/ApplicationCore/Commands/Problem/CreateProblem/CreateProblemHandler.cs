using ApplicationCore.Domain.Problems;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands.Problem.CreateProblem;

public sealed class CreateProblemHandler(
    IProblemRepository problemRepository,
    ISlugService slugService,
    IValidator<CreateProblemCommand> validator
) : AbstractCommandHandler<CreateProblemCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(
        CreateProblemCommand request,
        CancellationToken cancellationToken
    )
    {
        var problem = new ProblemModel
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Question = request.Question,
            Slug = slugService.GenerateSlug(request.Title),
            Tags = request.Tags.Select(tag => new TagModel { Value = tag }),
            CreatedById = request.CreatedById,
        };

        try
        {
            await problemRepository.CreateProblemAsync(problem, cancellationToken);

            return Result.Success(problem.Id);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}