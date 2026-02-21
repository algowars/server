using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using FluentValidation;

namespace ApplicationCore.Commands.Problem.CreateProblem;

public sealed class CreateProblemValidator : AbstractValidator<CreateProblemCommand>
{
    public CreateProblemValidator(IProblemRepository problemRepository, ISlugService slugService)
    {
        RuleFor(v => v.Title)
            .NotEmpty()
            .MustAsync(
                async (title, cancellationToken) =>
                    (
                        await problemRepository.GetProblemBySlugAsync(
                            slugService.GenerateSlug(title),
                            cancellationToken
                        )
                    ) == null
            )
            .WithMessage("A problem with this title already exists.");

        RuleFor(v => v.Question).NotEmpty();

        RuleFor(v => v.EstimatedDifficulty).GreaterThan(0).LessThan(3000);

        RuleFor(v => v.Tags).NotNull().NotEmpty();
    }
}