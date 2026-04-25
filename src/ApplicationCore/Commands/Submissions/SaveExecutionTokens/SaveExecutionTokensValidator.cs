using FluentValidation;

namespace ApplicationCore.Commands.Submissions.SaveExecutionTokens;

public sealed class SaveExecutionTokensValidator : AbstractValidator<SaveExecutionTokensCommand>
{
    public SaveExecutionTokensValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}
