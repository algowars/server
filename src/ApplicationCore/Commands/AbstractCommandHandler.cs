using Ardalis.Result;
using FluentValidation;

namespace ApplicationCore.Commands;

public abstract class AbstractCommandHandler<TCommand, TResult>(
    IValidator<TCommand>? validator = null
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IValidator<TCommand>? _validator = validator;

    public async Task<Result<TResult>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        if (_validator is not null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult
                    .Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                    .ToList();

                return Result<TResult>.Invalid(errors);
            }
        }

        return await HandleValidated(request, cancellationToken);
    }

    protected abstract Task<Result<TResult>> HandleValidated(
        TCommand request,
        CancellationToken cancellationToken
    );
}
