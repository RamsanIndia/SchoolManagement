using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Exceptions;

namespace SchoolManagement.Application.Behaviors
{
    /// <summary>
    /// Validates requests using FluentValidation before passing to the handler.
    /// Throws domain ValidationException so API can return ValidationProblemDetails.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count == 0)
                return await next();

            // Convert to field -> messages[] dictionary (ValidationProblemDetails-friendly)
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).Distinct().ToArray()
                );

            _logger.LogWarning(
                "Validation failed for {RequestType}. ErrorFields: {FieldCount}, TotalErrors: {ErrorCount}",
                typeof(TRequest).Name,
                errors.Count,
                failures.Count);

            // Throw domain exception (NOT FluentValidation.ValidationException)
            throw new Domain.Exceptions.ValidationException(errors);
        }
    }
}
