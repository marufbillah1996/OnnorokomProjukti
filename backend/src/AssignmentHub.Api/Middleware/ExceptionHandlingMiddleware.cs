using AssignmentHub.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Api.Middleware;

/// <summary>
/// Single place that turns every exception into an RFC 7807 ProblemDetails response, so
/// Application-layer services never construct an HTTP response themselves (see the exception
/// conventions documented in Application/DependencyInjection.cs's bounded contexts).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({StatusCode}) processing {Method} {Path}", statusCode, context.Request.Method, context.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        ForbiddenOperationException => (StatusCodes.Status403Forbidden, "Forbidden"),
        DeadlinePassedException => (StatusCodes.Status409Conflict, "Deadline passed"),
        DuplicateSubmissionException => (StatusCodes.Status409Conflict, "Duplicate submission"),
        InvalidMarksException => (StatusCodes.Status400BadRequest, "Invalid marks"),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict — the record was modified by someone else. Please reload and try again."),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error")
    };
}
