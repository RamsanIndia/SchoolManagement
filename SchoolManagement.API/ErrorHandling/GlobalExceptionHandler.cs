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

        var (statusCode, title) = exception switch
        {
            AuthenticationException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ValidationException => (HttpStatusCode.BadRequest, "Validation Error"),
            NotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] = _correlation.CorrelationId;

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true; // ✅ exception handled
    }
}
