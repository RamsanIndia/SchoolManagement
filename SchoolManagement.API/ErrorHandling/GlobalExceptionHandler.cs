using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Shared.Correlation;
using SchoolManagement.Domain.Exceptions;
using System.Net;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly ICorrelationIdAccessor _correlation;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        ICorrelationIdAccessor correlation)
    {
        _logger = logger;
        _correlation = correlation;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}",
            _correlation.CorrelationId);

        // Handle ValidationException with detailed field errors
        if (exception is ValidationException validationException)
        {
            var validationProblem = new ValidationProblemDetails(validationException.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = validationException.Message,
                Instance = context.Request.Path
            };

            validationProblem.Extensions["correlationId"] = _correlation.CorrelationId;
            validationProblem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = validationProblem.Status.Value;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

        // Handle other exceptions (only use exceptions that exist in your project)
        var (statusCode, title) = exception switch
        {
            AuthenticationException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            NotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        var problem = new ProblemDetails
        {
            Type = GetProblemType(statusCode),
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] = _correlation.CorrelationId;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        // Only include exception details in development
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            problem.Extensions["exceptionType"] = exception.GetType().Name;
        }

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static string GetProblemType(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
    };
}
