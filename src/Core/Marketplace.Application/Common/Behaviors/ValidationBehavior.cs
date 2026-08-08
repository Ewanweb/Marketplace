using FluentValidation;
using Marketplace.Shared.Results;
using MediatR;

namespace Marketplace.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new { validationFailure.PropertyName, validationFailure.ErrorMessage })
            .ToList();

        if (errors.Any())
        {
            var errorMessages = string.Join("; ", errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            var error = Error.Validation("ValidationError", errorMessages);
            
            return CreateValidationResult<TResponse>(error);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)Result.Failure(error);
        }

        var resultType = typeof(TResult).GetGenericArguments()[0];
        var failureMethod = typeof(Result<>)
            .MakeGenericType(resultType)
            .GetMethod(nameof(Result<int>.Failure))!;
            
        return (TResult)failureMethod.Invoke(null, new object[] { error })!;
    }
}
