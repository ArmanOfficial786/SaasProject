// File: Shared.Application/Behaviors/ValidationBehavior.cs
using FluentValidation;
using MediatR;

namespace Shared.Application.Behaviors;
//It's a MediatR pipeline behavior — it runs before every command/query handler, for every request type, automatically. Its only job is: stop bad input before it reaches business logic.
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new Shared.Application.Exceptions.ValidationException(failures);
            }
        }

        return await next();
    }
}
